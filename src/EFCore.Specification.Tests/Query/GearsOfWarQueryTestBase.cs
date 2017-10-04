// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;
// ReSharper disable InconsistentNaming
// ReSharper disable AccessToDisposedClosure
// ReSharper disable StringEndsWithIsCultureSpecific
// ReSharper disable ReplaceWithSingleCallToSingle

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Query
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
        public virtual void ToString_guid_property_projection()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Select(ct => new { A = ct.GearNickName, B = ct.Id.ToString() });
                var result = query.ToList();

                Assert.Equal(6, result.Count);
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
                        c => c.BornGears?.Select(g => g.Nickname).ToList() ?? new List<string>());

                cityStationedGears = context.Cities
                    .Include(c => c.StationedGears)
                    .ToDictionary(
                        c => c.Name,
                        c => c.StationedGears?.Select(g => g.Nickname).ToList() ?? new List<string>());
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
                    Assert.NotNull(result[i]);

                    Assert.Equal(gearAssignedCities[result[i].Nickname], result[i].AssignedCity?.Name);
                    Assert.Equal(gearCitiesOfBirth[result[i].Nickname], result[i].CityOfBirth?.Name);

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
        public virtual void String_based_Include_navigation_on_derived_type()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.OfType<Officer>().Include("Reports");
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
        public virtual void Include_where_list_contains_navigation2()
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

                var gears = context.Gears
                    .Include(g => g.Tag)
                    .Where(g => g.CityOfBirth.Location != null && tags.Contains(g.Tag.Id))
                    .ToList();

                Assert.Equal(5, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_accessed_twice_outside_and_inside_subquery()
        {
            using (var context = CreateContext())
            {
                context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;

                var tags = context.Tags.Select(t => (Guid?)t.Id).ToList();

                var gears = context.Gears
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
                Assert.Null(result[0].Owner?.CityOrBirthName);
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
        public virtual void Where_bitwise_and_enum()
        {
            using (var context = CreateContext())
            {
                var gears = context.Gears
                    .Where(g => (g.Rank & MilitaryRank.Corporal) > 0)
                    .ToList();

                Assert.Equal(2, gears.Count);

                gears = context.Gears
                    .Where(g => (g.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal)
                    .ToList();

                Assert.Equal(2, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_integral()
        {
            using (var context = CreateContext())
            {
                var gears = context.Gears
                    .Where(g => ((int)g.Rank & 1) == 1)
                    .ToList();

                Assert.Equal(2, gears.Count);

                gears = context.Gears
                    .Where(g => ((long)g.Rank & 1L) == 1L)
                    .ToList();

                Assert.Equal(2, gears.Count);

                gears = context.Gears
                    .Where(g => ((short)g.Rank & (short)1) == 1)
                    .ToList();

                Assert.Equal(2, gears.Count);

                gears = context.Gears
                    .Where(g => ((char)g.Rank & '\x0001') == '\x0001')
                    .ToList();

                Assert.Equal(2, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_nullable_enum_with_constant()
        {
            using (var context = CreateContext())
            {
                var weapons = context.Weapons
                    .Where(w => (w.AmmunitionType & AmmunitionType.Cartridge) > 0)
                    .ToList();

                Assert.Equal(5, weapons.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_nullable_enum_with_null_constant()
        {
            using (var context = CreateContext())
            {
                var weapons = context.Weapons
#pragma warning disable CS0458 // The result of the expression is always 'null'
                    .Where(w => (w.AmmunitionType & null) > 0)
#pragma warning restore CS0458 // The result of the expression is always 'null'
                    .ToList();

                Assert.Equal(0, weapons.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_nullable_enum_with_non_nullable_parameter()
        {
            var ammunitionType = AmmunitionType.Cartridge;

            using (var context = CreateContext())
            {
                var weapons = context.Weapons
                    .Where(w => (w.AmmunitionType & ammunitionType) > 0)
                    .ToList();

                Assert.Equal(5, weapons.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_bitwise_and_nullable_enum_with_nullable_parameter()
        {
            AmmunitionType? ammunitionType = AmmunitionType.Cartridge;

            using (var context = CreateContext())
            {
                var weapons = context.Weapons
                    .Where(w => (w.AmmunitionType & ammunitionType) > 0)
                    .ToList();

                Assert.Equal(5, weapons.Count);
            }

            ammunitionType = null;

            using (var context = CreateContext())
            {
                var weapons = context.Weapons
                    .Where(w => (w.AmmunitionType & ammunitionType) > 0)
                    .ToList();

                Assert.Equal(0, weapons.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_bitwise_or_enum()
        {
            using (var context = CreateContext())
            {
                var gears = context.Gears
                    .Where(g => (g.Rank | MilitaryRank.Corporal) > 0)
                    .ToList();

                Assert.Equal(5, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Bitwise_projects_values_in_select()
        {
            using (var context = CreateContext())
            {
                var gear = context.Gears
                    .Where(g => (g.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal)
                    .Select(b => new
                    {
                        BitwiseTrue = (b.Rank & MilitaryRank.Corporal) == MilitaryRank.Corporal,
                        BitwiseFalse = (b.Rank & MilitaryRank.Corporal) == MilitaryRank.Sergeant,
                        BitwiseValue = b.Rank & MilitaryRank.Corporal
                    }).First();

                Assert.True(gear.BitwiseTrue);
                Assert.False(gear.BitwiseFalse);
                Assert.True(gear.BitwiseValue == MilitaryRank.Corporal);
            }
        }

        [ConditionalFact]
        public virtual void Where_enum_has_flag()
        {
            using (var context = CreateContext())
            {
                // Constant
                var gears = context.Gears
                    .Where(g => g.Rank.HasFlag(MilitaryRank.Corporal))
                    .ToList();

                Assert.Equal(2, gears.Count);

                // Expression
                gears = context.Gears
                    .Where(g => g.Rank.HasFlag(MilitaryRank.Corporal | MilitaryRank.Captain))
                    .ToList();

                Assert.Equal(0, gears.Count);

                // Casting
                gears = context.Gears
                    .Where(g => g.Rank.HasFlag((MilitaryRank)1))
                    .ToList();

                Assert.Equal(2, gears.Count);

                // Casting to nullable
                gears = context.Gears
                    .Where(g => g.Rank.HasFlag((MilitaryRank?)1))
                    .ToList();

                Assert.Equal(2, gears.Count);

                // QuerySource
                gears = context.Gears
                    .Where(g => MilitaryRank.Corporal.HasFlag(g.Rank))
                    .ToList();

                Assert.Equal(4, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_enum_has_flag_subquery()
        {
            using (var context = CreateContext())
            {
                var gears = context.Gears
                    .Where(g => g.Rank.HasFlag(context.Gears.OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).FirstOrDefault().Rank))
                    .ToList();

                Assert.Equal(2, gears.Count);

                gears = context.Gears
                    .Where(g => MilitaryRank.Corporal.HasFlag(context.Gears.OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).FirstOrDefault().Rank))
                    .ToList();

                Assert.Equal(5, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_enum_has_flag_subquery_client_eval()
        {
            using (var context = CreateContext())
            {
                var gears = context.Gears
                    .Where(g => g.Rank.HasFlag(context.Gears.OrderBy(x => x.Nickname).ThenBy(x => x.SquadId).First().Rank))
                    .ToList();

                Assert.Equal(2, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_enum_has_flag_with_non_nullable_parameter()
        {
            using (var context = CreateContext())
            {
                var parameter = MilitaryRank.Corporal;

                var gears = context.Gears
                    .Where(g => g.Rank.HasFlag(parameter))
                    .ToList();

                Assert.Equal(2, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_has_flag_with_nullable_parameter()
        {
            using (var context = CreateContext())
            {
                MilitaryRank? parameter = MilitaryRank.Corporal;

                var gears = context.Gears
                    .Where(g => g.Rank.HasFlag(parameter))
                    .ToList();

                Assert.Equal(2, gears.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_enum_has_flag()
        {
            using (var context = CreateContext())
            {
                var gear = context.Gears
                    .Where(g => g.Rank.HasFlag(MilitaryRank.Corporal))
                    .Select(b => new
                    {
                        hasFlagTrue = b.Rank.HasFlag(MilitaryRank.Corporal),
                        hasFlagFalse = b.Rank.HasFlag(MilitaryRank.Sergeant)
                    }).First();

                Assert.True(gear.hasFlagTrue);
                Assert.False(gear.hasFlagFalse);
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
                    .Where(g => (g == null ? null : g.LeaderNickname) == "Marcus" == (bool?)true)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(3, result.Count);

                var nickNames = result.Select(r => r.Nickname).ToList();
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

                var nickNames = result.Select(r => r.Nickname).ToList();
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

                var nickNames = result.Select(r => r.Nickname).ToList();
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
                    .Where(g => (null == EF.Property<string>(g, "LeaderNickname") ? (int?)null : g.LeaderNickname.Length) == 5 == (bool?)true)
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
                    .Where(g => (null != g.LeaderNickname ? (int?)(EF.Property<string>(g, "LeaderNickname").Length) : (int?)null) == 5 == (bool?)true)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(1, result.Count);

                var nickNames = result.Select(r => r.Nickname);
                Assert.True(nickNames.Contains("Paduk"));
            }
        }

        [ConditionalFact]
        public virtual void Null_propagation_optimization6()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Where(g => (null != g.LeaderNickname ? (int?)EF.Property<string>(g, "LeaderNickname").Length : (int?)null) == 5 == (bool?)true)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(1, result.Count);

                var nickNames = result.Select(r => r.Nickname);
                Assert.True(nickNames.Contains("Paduk"));
            }
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_optimization7()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Select(g => null != g.LeaderNickname ? g.LeaderNickname + g.LeaderNickname : null)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(5, result.Count);
                Assert.True(result.Contains(null));
            }
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_optimization8()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Select(g => g != null ? g.LeaderNickname + g.LeaderNickname : null)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(5, result.Count);
                Assert.True(result.Any(string.IsNullOrEmpty));
            }
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_optimization9()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Select(g => g != null ? (int?)g.FullName.Length : (int?)null)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(5, result.Count);
                Assert.Equal(1, result.Count(r => r == 16));
                Assert.Equal(1, result.Count(r => r == 13));
                Assert.Equal(2, result.Count(r => r == 12));
                Assert.Equal(1, result.Count(r => r == 11));
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
        public virtual void Select_null_propagation_negative3()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears
                            join g2 in context.Gears on g1.HasSoulPatch equals true into grouping
                            from g2 in grouping.DefaultIfEmpty()
                            orderby g2.Nickname
                            select new { g2.Nickname, Condition = g2 != null ? (bool?)(g2.LeaderNickname != null) : (bool?)null };

                var result = query.ToList();

                Assert.Equal(13, result.Count);
                Assert.Null(result[0].Nickname);
                Assert.Null(result[0].Condition);

                Assert.Equal("Baird", result[3].Nickname);
                Assert.True(result[3].Condition);

                Assert.Equal("Marcus", result[9].Nickname);
                Assert.False(result[9].Condition);
            }
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative4()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears
                            join g2 in context.Gears on g1.HasSoulPatch equals true into grouping
                            from g2 in grouping.DefaultIfEmpty()
                            orderby g2.Nickname
                            select g2 != null ? new Tuple<string, int>(g2.Nickname, 5) : null;

                var result = query.ToList();

                Assert.Equal(13, result.Count);
                Assert.Null(result[0]);
                Assert.Equal("Baird", result[3].Item1);
                Assert.Equal(5, result[3].Item2);
            }
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative5()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears
                            join g2 in context.Gears on g1.HasSoulPatch equals true into grouping
                            from g2 in grouping.DefaultIfEmpty()
                            orderby g2.Nickname
                            select g2 != null ? new { g2.Nickname, Five = 5 } : null;

                var result = query.ToList();
                Assert.Equal(13, result.Count);
                Assert.Null(result[0]);
                Assert.Equal("Baird", result[3].Nickname);
                Assert.Equal(5, result[3].Five);
            }
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative6()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Select(g => null != g.LeaderNickname ? EF.Property<string>(g, "LeaderNickname").Length != EF.Property<string>(g, "LeaderNickname").Length : (bool?)null)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(5, result.Count);
                Assert.True(result.Contains(null));
            }
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative7()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Select(g => null != g.LeaderNickname ? g.LeaderNickname == g.LeaderNickname : (bool?)null)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(5, result.Count);
                Assert.True(result.Contains(null));
            }
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_negative8()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags
                    .Select(t => t.Gear.Squad != null ? t.Gear.AssignedCity.Name : null)
                    .ToList();

                var result = query.ToList();

                Assert.Equal(6, result.Count);
                Assert.True(result.Contains(null));
            }
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_works_for_navigations_with_composite_keys()
        {
            using (var context = CreateContext())
            {
                var query = from t in context.Tags
                            select t.Gear != null ? t.Gear.Nickname : null;

                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_null_propagation_works_for_multiple_navigations_with_composite_keys()
        {
            using (var context = CreateContext())
            {
                var query = from t in context.Tags
                            select EF.Property<City>(EF.Property<CogTag>(t.Gear, "Tag").Gear, "AssignedCity") != null 
                                ? EF.Property<string>(EF.Property<Gear>(t.Gear.Tag, "Gear").AssignedCity, "Name") 
                                : null;

                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_conditional_with_anonymous_type_and_null_constant()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            orderby g.Nickname
                            select g.LeaderNickname != null ? new { g.HasSoulPatch } : null;

                var result = query.ToList();
                Assert.Equal(5, result.Count);
                Assert.True(result[0].HasSoulPatch);
                Assert.False(result[1].HasSoulPatch);
                Assert.False(result[2].HasSoulPatch);
                Assert.Null(result[3]);
                Assert.False(result[4].HasSoulPatch);
            }
        }

        [ConditionalFact]
        public virtual void Select_conditional_with_anonymous_types()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            orderby g.Nickname
                            select g.LeaderNickname != null ? new { Name = g.Nickname } : new { Name = g.FullName };

                var result = query.ToList();
                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_conditional_with_anonymous_type()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            orderby g.Nickname
                            where (g.LeaderNickname != null ? new { g.HasSoulPatch } : null) == null
                            select g.Nickname;

                var result = query.ToList();
                Assert.Equal(1, result.Count);
                Assert.Equal("Marcus", result[0]);
            }
        }

        [ConditionalFact]
        public virtual void Select_coalesce_with_anonymous_types()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            orderby g.Nickname
                            // ReSharper disable once ConstantNullCoalescingCondition
                            select new { Name = g.LeaderNickname } ?? new { Name = g.FullName };

                var result = query.ToList();
                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_coalesce_with_anonymous_types()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            // ReSharper disable once ConstantNullCoalescingCondition
                            // ReSharper disable once ConstantNullCoalescingCondition
                            // ReSharper disable once ConstantNullCoalescingCondition
                            where (new { Name = g.LeaderNickname } ?? new { Name = g.FullName }) != null
                            select g.Nickname;

                var result = query.ToList();
                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact(Skip = "issue #8421")]
        public virtual void Where_compare_anonymous_types()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            from o in context.Gears.OfType<Officer>()
                            where new { Name = g.LeaderNickname, Squad = g.LeaderSquadId, Five = 5 } == new { Name = o.Nickname, Squad = o.SquadId, Five = 5 }
                            select g.Nickname;

                var result = query.ToList();
                Assert.Equal(4, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_member_access_on_anonymous_type()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            where new { Name = g.LeaderNickname, Squad = g.LeaderSquadId }.Name == "Marcus"
                            select g.Nickname;

                var result = query.ToList();
                Assert.Equal(3, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_compare_anonymous_types_with_uncorrelated_members()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            // ReSharper disable once EqualExpressionComparison
                            where new { Five = 5 } == new { Five = 5 }
                            select g.Nickname;

                var result = query.ToList();
                Assert.Equal(0, result.Count);
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

        [ConditionalFact]
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

        [ConditionalFact]
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
        public virtual void Where_subquery_boolean()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Where(g => g.Weapons.FirstOrDefault().IsAutomatic).ToList();

                Assert.Equal(2, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_subquery_distinct_firstordefault_boolean()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Where(g => g.HasSoulPatch && g.Weapons.Distinct().FirstOrDefault().IsAutomatic);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.True(result.Select(r => r.Nickname).Contains("Marcus"));
                Assert.True(result.Select(r => r.Nickname).Contains("Baird"));
            }
        }

        [ConditionalFact]
        public virtual void Where_subquery_distinct_first_boolean()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.OrderBy(g => g.Nickname).Where(g => g.HasSoulPatch && g.Weapons.Distinct().First().IsAutomatic);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Baird", result[0].Nickname);
                Assert.Equal("Marcus", result[1].Nickname);
            }
        }

        [ConditionalFact]
        public virtual void Where_subquery_distinct_singleordefault_boolean()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .OrderBy(g => g.Nickname)
                    .Where(g => g.HasSoulPatch && g.Weapons.Where(w => w.Name.Contains("Lancer")).Distinct().SingleOrDefault().IsAutomatic);

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Baird", result[0].Nickname);
                Assert.Equal("Marcus", result[1].Nickname);
            }
        }

        [ConditionalFact(Skip = "issue #8582")]
        public virtual void Where_subquery_distinct_lastordefault_boolean()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .OrderBy(g => g.Nickname)
                    .Where(g => !g.Weapons.Distinct().OrderBy(w => w.Id).LastOrDefault().IsAutomatic);

                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.Equal("Baird", result[0].Nickname);
                Assert.Equal("Dom", result[1].Nickname);
                Assert.Equal("Marcus", result[2].Nickname);
                Assert.Equal("Paduk", result[3].Nickname);
            }
        }

        [ConditionalFact(Skip = "issue #8582")]
        public virtual void Where_subquery_distinct_last_boolean()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .OrderBy(g => g.Nickname)
                    .Where(g => !g.HasSoulPatch && g.Weapons.Distinct().Last().IsAutomatic);

                var result = query.ToList();

                Assert.Equal(1, result.Count);
                Assert.Equal("Cole Train", result[0].Nickname);
            }
        }

        [ConditionalFact]
        public virtual void Where_subquery_distinct_orderby_firstordefault_boolean()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Where(g => g.HasSoulPatch && g.Weapons.Distinct().OrderBy(w => w.Id).FirstOrDefault().IsAutomatic);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.True(result.Select(r => r.Nickname).Contains("Marcus"));
                Assert.True(result.Select(r => r.Nickname).Contains("Baird"));
            }
        }

        [ConditionalFact]
        public virtual void Where_subquery_union_firstordefault_boolean()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Where(g => g.HasSoulPatch && g.Weapons.Union(g.Weapons).FirstOrDefault().IsAutomatic).ToList();

                Assert.Equal(2, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_subquery_concat_firstordefault_boolean()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Where(g => g.HasSoulPatch && g.Weapons.Concat(g.Weapons).FirstOrDefault().IsAutomatic);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Concat_with_count()
        {
            using (var context = CreateContext())
            {
                var result = context.Gears.Concat(context.Gears).Count();

                Assert.Equal(10, result);
            }
        }

        [ConditionalFact]
        public virtual void Concat_scalars_with_count()
        {
            using (var context = CreateContext())
            {
                var result = context.Gears.Select(g => g.Nickname).Concat(context.Gears.Select(g2 => g2.FullName)).Count();
            }
        }

        [ConditionalFact]
        public virtual void Concat_anonymous_with_count()
        {
            using (var context = CreateContext())
            {
                var result = context.Gears.Select(g => new { Gear = g, Name = g.Nickname })
                    .Concat(context.Gears.Select(g2 => new { Gear = g2, Name = g2.FullName })).Count();

                Assert.Equal(10, result);
            }
        }

        [ConditionalFact(Skip = "issue #9007")]
        public virtual void Concat_with_scalar_projection()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Concat(context.Gears).Select(g => g.Nickname);
                var result = query.ToList();

                Assert.Equal(10, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Concat_with_groupings()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.GroupBy(g => g.LeaderNickname).Concat(context.Gears.GroupBy(g => g.LeaderNickname));
                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_navigation_with_concat_and_count()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Where(g => !g.HasSoulPatch).Select(g => g.Weapons.Concat(g.Weapons).Count()).ToList();
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Where_subquery_concat_order_by_firstordefault_boolean()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Where(g => g.Weapons.Concat(g.Weapons).OrderBy(w => w.Id).FirstOrDefault().IsAutomatic);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Concat_with_collection_navigations()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Where(g => g.HasSoulPatch).Select(g => g.Weapons.Union(g.Weapons).Count());
                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Union_with_collection_navigations()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.OfType<Officer>().Select(o => o.Reports.Union(o.Reports).Count());
                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_subquery_distinct_firstordefault()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.Where(g => g.HasSoulPatch).Select(g => g.Weapons.Distinct().FirstOrDefault().Name);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.True(result.Contains("Baird's Lancer"));
                Assert.True(result.Contains("Marcus' Lancer"));
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

        [ConditionalFact]
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
                            where c.Location == "Unknown" && c.BornGears.Count(g => g.Nickname == "Paduk") == 1
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

        [ConditionalFact(Skip = "Test does not pass. See issue#4978")]
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

                Assert.Equal("Marcus", result[0].Nickname);
                Assert.Equal(2, result[0].Weapons.Count);
                Assert.Equal("Marcus", result[1].Nickname);
                Assert.Equal("Marcus", result[2].Nickname);
                Assert.Equal("Baird", result[3].Nickname);
                Assert.Equal(0, result[3].Weapons.Count);
                Assert.Equal("Marcus", result[4].Nickname);
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

                Assert.Equal("Marcus", result[0].Nickname);
                Assert.Equal(2, result[0].Weapons.Count);
                Assert.Equal("Baird", result[1].Nickname);
                Assert.Equal(2, result[1].Weapons.Count);
                Assert.Equal("Marcus", result[2].Nickname);
                Assert.Equal("Marcus", result[3].Nickname);
                Assert.Equal("Marcus", result[4].Nickname);
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

                Assert.Equal("Marcus", result[0].Nickname);
                Assert.Equal(2, result[0].Weapons.Count);
                Assert.Equal("Baird", result[1].Nickname);
                Assert.Equal(2, result[1].Weapons.Count);
                Assert.Equal("Marcus", result[2].Nickname);
                Assert.Equal("Marcus", result[3].Nickname);
                Assert.Equal("Marcus", result[4].Nickname);
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
                            // ReSharper disable once MergeConditionalExpression
                            select g2 != null ? g2 : g1;

                var result = query.ToList();

                Assert.Equal("Marcus", result[0].Nickname);
                Assert.Equal(2, result[0].Weapons.Count);
                Assert.Equal("Baird", result[1].Nickname);
                Assert.Equal(2, result[1].Weapons.Count);
                Assert.Equal("Marcus", result[2].Nickname);
                Assert.Equal("Marcus", result[3].Nickname);
                Assert.Equal("Marcus", result[4].Nickname);
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
                            // ReSharper disable once MergeConditionalExpression
                            select new { g1, g2, coalesce = g2 ?? g1, conditional = g2 != null ? g2 : g1 };

                var result = query.ToList();

                Assert.True(result.All(r => r.coalesce.Weapons.Count > 0));
                Assert.True(result.All(r => r.conditional.Weapons.Count > 0));
            }
        }

        [ConditionalFact]
        public virtual void Coalesce_operator_in_predicate()
        {
            using (var context = CreateContext())
            {
                var query = context.Weapons.Where(w => (bool?)w.IsAutomatic ?? false).ToList();

                Assert.Equal(3, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Coalesce_operator_in_predicate_with_other_conditions()
        {
            using (var context = CreateContext())
            {
                var query = context.Weapons.Where(w => w.AmmunitionType == AmmunitionType.Cartridge && ((bool?)w.IsAutomatic ?? false)).ToList();

                Assert.Equal(3, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Coalesce_operator_in_projection_with_other_conditions()
        {
            using (var context = CreateContext())
            {
                var query = context.Weapons.Select(w => w.AmmunitionType == AmmunitionType.Cartridge && ((bool?)w.IsAutomatic ?? false)).ToList();

                Assert.Equal(10, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_predicate()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Note != "K.I.A." && t.Gear.HasSoulPatch);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_predicate2()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Gear.HasSoulPatch);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_predicate_negated()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => !t.Gear.HasSoulPatch);
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_predicate_negated_complex1()
        {
            using (var context = CreateContext())
            {
                // ReSharper disable once SimplifyConditionalTernaryExpression
                var query = context.Tags.Where(t => !(t.Gear.HasSoulPatch ? true : t.Gear.HasSoulPatch));
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_predicate_negated_complex2()
        {
            using (var context = CreateContext())
            {
                // ReSharper disable once SimplifyConditionalTernaryExpression
                // ReSharper disable once SimplifyConditionalTernaryExpression
                // ReSharper disable once SimplifyConditionalTernaryExpression
                var query = context.Tags.Where(t => !(!t.Gear.HasSoulPatch ? false : t.Gear.HasSoulPatch));
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_conditional_expression()
        {
            using (var context = CreateContext())
            {
                // ReSharper disable once RedundantTernaryExpression
                var query = context.Tags.Where(t => t.Gear.HasSoulPatch ? true : false);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_binary_expression()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Gear.HasSoulPatch || t.Note.Contains("Cole"));
                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_projection()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Note != "K.I.A.").Select(t => t.Gear.SquadId);
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_projection_into_anonymous_type()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Note != "K.I.A.").Select(t => new { t.Gear.SquadId });
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_DTOs()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Note != "K.I.A.").Select(t => new Squad { Id = t.Gear.SquadId });
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_list_initializers()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Note != "K.I.A.").Select(t => new List<int> { t.Gear.SquadId, t.Gear.SquadId + 1, 42 });
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_array_initializers()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Note != "K.I.A.").Select(t => new[] { t.Gear.SquadId });
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_orderby()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Note != "K.I.A.").OrderBy(t => t.Gear.SquadId);
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_groupby()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Note != "K.I.A.").GroupBy(t => t.Gear.SquadId);
                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_all()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Note != "K.I.A.").All(t => t.Gear.HasSoulPatch);

                Assert.False(query);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_contains()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Note != "K.I.A." && context.Gears.Select(g => g.SquadId).Contains(t.Gear.SquadId));
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_skip()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Note != "K.I.A.").Select(t => context.Gears.Skip(t.Gear.SquadId));
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_type_compensation_works_with_take()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Note != "K.I.A.").Select(t => context.Gears.Take(t.Gear.SquadId));
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Select_correlated_filtered_collection()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .Where(g => g.CityOfBirth.Name == "Ephyra" || g.CityOfBirth.Name == "Hanover")
                    .Select(g => g.Weapons.Where(w => w.Name != "Lancer"));
                var result = query.ToList();

                Assert.Equal(2, result.Count);

                var resultList = result.Select(r => r.ToList()).ToList();
                var coleWeapons = resultList.Where(l => l.All(w => w.Name.Contains("Cole's"))).Single();
                var domWeapons = resultList.Where(l => l.All(w => w.Name.Contains("Dom's"))).Single();

                Assert.Equal(2, coleWeapons.Count);
                Assert.True(coleWeapons.Select(w => w.Name).Contains("Cole's Gnasher"));

                Assert.Equal(2, domWeapons.Count);
                Assert.True(domWeapons.Select(w => w.Name).Contains("Dom's Hammerburst"));
                Assert.True(domWeapons.Select(w => w.Name).Contains("Dom's Gnasher"));
            }
        }

        [ConditionalFact]
        public virtual void Select_correlated_filtered_collection_with_composite_key()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.OfType<Officer>().Select(g => g.Reports.Where(r => r.Nickname != "Dom"));
                var result = query.ToList();

                Assert.Equal(2, result.Count);

                var resultList = result.Select(r => r.ToList()).ToList();
                var bairdReports = resultList.Where(l => l.Count == 1).Single();
                var marcusReports = resultList.Where(l => l.Count == 2).Single();

                Assert.True(bairdReports.Select(g => g.FullName).Contains("Garron Paduk"));
                Assert.True(marcusReports.Select(g => g.FullName).Contains("Augustus Cole"));
                Assert.True(marcusReports.Select(g => g.FullName).Contains("Damon Baird"));
            }
        }

        [ConditionalFact]
        public virtual void Select_correlated_filtered_collection_works_with_caching()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Select(t => context.Gears.Where(g => g.Nickname == t.GearNickName));
                var result = query.ToList();

                // ReSharper disable once UnusedVariable
                var resultList = result.Select(r => r.ToList()).ToList();
            }
        }

        [ConditionalFact]
        public virtual void Join_predicate_value_equals_condition()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            join w in context.Weapons
                            on true equals w.SynergyWithId != null
                            select g;
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Join_predicate_value()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            join w in context.Weapons
                            on g.HasSoulPatch equals true
                            select g;
                var result = query.ToList();

                Assert.Equal(20, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Join_predicate_condition_equals_condition()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            join w in context.Weapons
                            on g.FullName != null equals w.SynergyWithId != null
                            select g;
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Left_join_predicate_value_equals_condition()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            join w in context.Weapons
                            on true equals w.SynergyWithId != null
                            into group1
                            from w in group1.DefaultIfEmpty()
                            select g;
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Left_join_predicate_value()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            join w in context.Weapons
                            on g.HasSoulPatch equals true
                            into group1
                            from w in group1.DefaultIfEmpty()
                            select g;
                var result = query.ToList();

                Assert.Equal(23, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Left_join_predicate_condition_equals_condition()
        {
            using (var context = CreateContext())
            {
                var query = from g in context.Gears
                            join w in context.Weapons
                            on g.FullName != null equals w.SynergyWithId != null
                            into group1
                            from w in group1.DefaultIfEmpty()
                            select g;
                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_Date_works()
        {
            using (var context = CreateContext())
            {
                var query = from m in context.Missions
                            where m.Timeline.Date > new DateTimeOffset().Date
                            select m;

                var result = query.ToList();

                Assert.Equal(3, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_Datepart_works()
        {
            using (var context = CreateContext())
            {
                var query = from m in context.Missions
                            where m.Timeline.Month == 5
                            select m;

                var result = query.ToList();

                Assert.Equal(1, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddYears()
        {
            using (var context = CreateContext())
            {
                var query = from m in context.Missions
                            orderby m.Timeline
                            select m.Timeline.AddYears(1);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal(new DateTimeOffset(3, 1, 2, 10, 0, 0, new TimeSpan(1, 30, 0)), result[0]);
                Assert.Equal(new DateTimeOffset(3, 3, 1, 8, 0, 0, new TimeSpan(-5, 0, 0)), result[1]);
                Assert.Equal(new DateTimeOffset(11, 5, 3, 12, 0, 0, new TimeSpan()), result[2]);
            }
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddMonths()
        {
            using (var context = CreateContext())
            {
                var query = from m in context.Missions
                            orderby m.Timeline
                            select m.Timeline.AddMonths(1);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal(new DateTimeOffset(2, 2, 2, 10, 0, 0, new TimeSpan(1, 30, 0)), result[0]);
                Assert.Equal(new DateTimeOffset(2, 4, 1, 8, 0, 0, new TimeSpan(-5, 0, 0)), result[1]);
                Assert.Equal(new DateTimeOffset(10, 6, 3, 12, 0, 0, new TimeSpan()), result[2]);
            }
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddDays()
        {
            using (var context = CreateContext())
            {
                var query = from m in context.Missions
                            orderby m.Timeline
                            select m.Timeline.AddDays(1);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal(new DateTimeOffset(2, 1, 3, 10, 0, 0, new TimeSpan(1, 30, 0)), result[0]);
                Assert.Equal(new DateTimeOffset(2, 3, 2, 8, 0, 0, new TimeSpan(-5, 0, 0)), result[1]);
                Assert.Equal(new DateTimeOffset(10, 5, 4, 12, 0, 0, new TimeSpan()), result[2]);
            }
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddHours()
        {
            using (var context = CreateContext())
            {
                var query = from m in context.Missions
                            orderby m.Timeline
                            select m.Timeline.AddHours(1);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal(new DateTimeOffset(2, 1, 2, 11, 0, 0, new TimeSpan(1, 30, 0)), result[0]);
                Assert.Equal(new DateTimeOffset(2, 3, 1, 9, 0, 0, new TimeSpan(-5, 0, 0)), result[1]);
                Assert.Equal(new DateTimeOffset(10, 5, 3, 13, 0, 0, new TimeSpan()), result[2]);
            }
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddMinutes()
        {
            using (var context = CreateContext())
            {
                var query = from m in context.Missions
                            orderby m.Timeline
                            select m.Timeline.AddMinutes(1);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal(new DateTimeOffset(2, 1, 2, 10, 1, 0, new TimeSpan(1, 30, 0)), result[0]);
                Assert.Equal(new DateTimeOffset(2, 3, 1, 8, 1, 0, new TimeSpan(-5, 0, 0)), result[1]);
                Assert.Equal(new DateTimeOffset(10, 5, 3, 12, 1, 0, new TimeSpan()), result[2]);
            }
        }

        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddSeconds()
        {
            using (var context = CreateContext())
            {
                var query = from m in context.Missions
                            orderby m.Timeline
                            select m.Timeline.AddSeconds(1);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal(new DateTimeOffset(2, 1, 2, 10, 0, 1, new TimeSpan(1, 30, 0)), result[0]);
                Assert.Equal(new DateTimeOffset(2, 3, 1, 8, 0, 1, new TimeSpan(-5, 0, 0)), result[1]);
                Assert.Equal(new DateTimeOffset(10, 5, 3, 12, 0, 1, new TimeSpan()), result[2]);
            }
        }


        [ConditionalFact]
        public virtual void DateTimeOffset_DateAdd_AddMilliseconds()
        {
            using (var context = CreateContext())
            {
                var query = from m in context.Missions
                            orderby m.Timeline
                            select m.Timeline.AddMilliseconds(300);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal(new DateTimeOffset(2, 1, 2, 10, 0, 0, 300, new TimeSpan(1, 30, 0)), result[0]);
                Assert.Equal(new DateTimeOffset(2, 3, 1, 8, 0, 0, 300, new TimeSpan(-5, 0, 0)), result[1]);
                Assert.Equal(new DateTimeOffset(10, 5, 3, 12, 0, 0, 300, new TimeSpan()), result[2]);
            }
        }

        [ConditionalFact]
        public virtual void Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used()
        {
            using (var ctx = CreateContext())
            {
                var query = from t in ctx.Tags
                            join g in ctx.Gears on t.GearNickName equals g.Nickname into grouping
                            from g in ClientDefaultIfEmpty(grouping)
                            select new { t.Note, Nickname = g != null ? g.Nickname : null };

                var result = query.ToList();

                Assert.Equal(6, result.Count);
            }
        }

        private static IEnumerable<TElement> ClientDefaultIfEmpty<TElement>(IEnumerable<TElement> source)
        {
            // ReSharper disable PossibleMultipleEnumeration
            return source?.Count() == 0 ? new[] { default(TElement) } : source;
            // ReSharper restore PossibleMultipleEnumeration
        }

        [ConditionalFact]
        public virtual void Complex_predicate_with_AndAlso_and_nullable_bool_property()
        {
            using (var context = CreateContext())
            {
                var query = from w in context.Weapons
                            where w.Id != 50 && !w.Owner.HasSoulPatch
                            select w;

                var result = query.ToList();

                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Distinct_with_optional_navigation_is_translated_to_sql()
        {
            using (var context = CreateContext())
            {
                var query = (from g in context.Gears
                             where g.Tag.Note != "Foo"
                             select g.HasSoulPatch).Distinct();

                var result = query.ToList();
                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Sum_with_optional_navigation_is_translated_to_sql()
        {
            using (var context = CreateContext())
            {
                var expected = (from g in context.Gears.ToList()
                                select g.SquadId).Sum();

                ClearLog();

                var actual = (from g in context.Gears
                              where g.Tag.Note != "Foo"
                              select g.SquadId).Sum();

                Assert.Equal(expected, actual);
            }
        }

        [ConditionalFact]
        public virtual void Count_with_optional_navigation_is_translated_to_sql()
        {
            using (var context = CreateContext())
            {
                var query = (from g in context.Gears
                             where g.Tag.Note != "Foo"
                             select g.HasSoulPatch).Count();

                Assert.Equal(5, query);
            }
        }

        [ConditionalFact]
        public virtual void Distinct_with_unflattened_groupjoin_is_evaluated_on_client()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.GroupJoin(
                    context.Tags,
                    g => new { k1 = g.Nickname, k2 = (int?)g.SquadId },
                    t => new { k1 = t.GearNickName, k2 = t.GearSquadId },
                    (g, t) => g.HasSoulPatch).Distinct();

                var result = query.ToList();

                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Count_with_unflattened_groupjoin_is_evaluated_on_client()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears.GroupJoin(
                    context.Tags,
                    g => new { k1 = g.Nickname, k2 = (int?)g.SquadId },
                    t => new { k1 = t.GearNickName, k2 = t.GearSquadId },
                    (g, t) => g).Count();

                Assert.Equal(5, query);
            }
        }

        [ConditionalFact]
        public virtual void FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql()
        {
            using (var context = CreateContext())
            {
                var query = (from s in context.Squads
                             join g in context.Gears on s.Id equals g.SquadId into grouping
                             from g in grouping.DefaultIfEmpty()
                             where s.Name == "Kilo"
                             select s).FirstOrDefault();

                Assert.Equal("Kilo", query?.Name);
            }
        }

        [ConditionalFact]
        public virtual void Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql()
        {
            using (var context = CreateContext())
            {
                var query = from s in context.Squads
                            // ReSharper disable once SimplifyLinqExpression
                            where !s.Members.Any(m => m.Tag.Note == "Dom's Tag")
                            select s.Name;

                var result = query.ToList();

                Assert.Equal("Kilo", result.Single());
            }
        }

        [ConditionalFact]
        public virtual void All_with_optional_navigation_is_translated_to_sql()
        {
            using (var context = CreateContext())
            {
                var query = (from g in context.Gears
                             select g).All(g => g.Tag.Note != "Foo");

                Assert.True(query);
            }
        }

        [ConditionalFact]
        public virtual void Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.GroupJoin(
                    context.Gears,
                    t => new { k1 = t.GearNickName, k2 = t.GearSquadId },
                    g => new { k1 = g.Nickname, k2 = (int?)g.SquadId },
                    (k, r) => r.Count());

                var result = query.ToList();

                Assert.Equal(6, result.Count);
                Assert.Equal(5, result.Count(r => r == 1));
                Assert.Equal(1, result.Count(r => r == 0));
            }
        }

        [ConditionalFact]
        public virtual void Client_side_equality_with_parameter_works_with_optional_navigations()
        {
            using (var context = CreateContext())
            {
                var prm = "Marcus's Tag";
                var query = context.Gears.Where(g => ClientEquals(g.Tag.Note, prm));

                var result = query.ToList();

                Assert.Equal(1, result.Count);
                Assert.Equal("Marcus", result[0].Nickname);
            }
        }

        private static bool ClientEquals(string first, string second)
            => first == second;

        [ConditionalFact]
        public virtual void Contains_with_local_nullable_guid_list_closure()
        {
            using (var context = CreateContext())
            {
                var ids = new List<Guid?>
                {
                    Guid.Parse("D2C26679-562B-44D1-AB96-23D1775E0926"),
                    Guid.Parse("23CBCF9B-CE14-45CF-AAFA-2C2667EBFDD3"),
                    Guid.Parse("AB1B82D7-88DB-42BD-A132-7EEF9AA68AF4")
                };

                var query = context.Tags.Where(e => ids.Contains(e.Id)).ToList();

                // Guids generated are random on each iteration.
                Assert.Equal(0, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .OrderBy(g => g.Rank)
                    .Include(g => g.Tag)
                    .Where(g => g.HasSoulPatch)
                    .Select(g => new { FullName = EF.Property<string>(g, "FullName") });

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Damon Baird", result[0].FullName);
                Assert.Equal("Marcus Fenix", result[1].FullName);
            }
        }

        [ConditionalFact]
        public virtual void Order_by_is_properly_lifted_from_subquery_created_by_include()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .OrderBy(g => g.Rank)
                    .Include(g => g.Tag)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g.FullName);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal("Augustus Cole", result[0]);
                Assert.Equal("Dominic Santiago", result[1]);
                Assert.Equal("Garron Paduk", result[2]);
            }
        }

        [ConditionalFact]
        public virtual void Order_by_then_by_is_properly_lifted_from_subquery_created_by_include()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .OrderBy(g => g.Rank).ThenByDescending(g => g.Nickname)
                    .Include(g => g.Tag)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g.FullName);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal("Augustus Cole", result[0]);
                Assert.Equal("Dominic Santiago", result[1]);
                Assert.Equal("Garron Paduk", result[2]);
            }
        }

        [ConditionalFact]
        public virtual void Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .OrderBy(g => g.Rank)
                    .Include(g => g.Tag)
                    .OrderByDescending(g => g.Nickname)
                    .Include(g => g.CityOfBirth)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g.FullName);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal("Augustus Cole", result[0]);
                Assert.Equal("Dominic Santiago", result[1]);
                Assert.Equal("Garron Paduk", result[2]);
            }
        }

        [ConditionalFact]
        public virtual void Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .OrderBy(g => g.FullName)
                    .Include(g => g.CityOfBirth)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g.FullName);

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal("Augustus Cole", result[0]);
                Assert.Equal("Dominic Santiago", result[1]);
                Assert.Equal("Garron Paduk", result[2]);
            }
        }

        [ConditionalFact]
        public virtual void Where_is_properly_lifted_from_subquery_created_by_include()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .Where(g => g.FullName != "Augustus Cole")
                    .Include(g => g.Tag)
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g);

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Dominic Santiago", result[0].FullName);
                Assert.Equal("Dom's Tag", result[0].Tag.Note);
                Assert.Equal("Garron Paduk", result[1].FullName);
                Assert.Equal("Paduk's Tag", result[1].Tag.Note);
            }
        }

        [ConditionalFact]
        public virtual void Where_and_order_by_are_properly_lifted_from_subquery_created_by_tracking()
        {
            using (var ctx = CreateContext())
            {
                var query = ctx.Gears
                    .Where(g => g.FullName != "Augustus Cole")
                    .AsNoTracking()
                    .OrderBy(g => g.Rank)
                    .AsTracking()
                    .OrderBy(g => g.FullName)
                    .Where(g => !g.HasSoulPatch)
                    .Select(g => g.FullName);

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Dominic Santiago", result[0]);
                Assert.Equal("Garron Paduk", result[1]);
            }
        }

        [ConditionalFact]
        public virtual void Subquery_is_lifted_from_main_from_clause_of_SelectMany()
        {
            using (var ctx = CreateContext())
            {
                var query = from g1 in ctx.Gears.OrderBy(g => g.Rank).Include(g => g.Tag)
                            from g2 in ctx.Gears
                            orderby g1.FullName
                            where g1.HasSoulPatch && !g2.HasSoulPatch
                            select new { Name1 = g1.FullName, Name2 = g2.FullName };

                var result = query.ToList();

                Assert.Equal(6, result.Count);
                Assert.True(result.All(r => r.Name1 == "Damon Baird" || r.Name1 == "Marcus Fenix"));
                Assert.True(result.All(r => r.Name2 == "Augustus Cole" || r.Name2 == "Garron Paduk" || r.Name2 == "Dominic Santiago"));
            }
        }

        [ConditionalFact]
        public virtual void Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted()
        {
            using (var ctx = CreateContext())
            {
                var query = from g in (from gear in ctx.Gears
                                       from tag in ctx.Tags
                                       where gear.HasSoulPatch
                                       orderby tag.Note
                                       select gear).AsTracking()
                            orderby g.FullName
                            select g.FullName;

                var result = query.ToList();

                Assert.Equal(12, result.Count);
                Assert.Equal(6, result.Count(r => r == "Damon Baird"));
                Assert.Equal(6, result.Count(r => r == "Marcus Fenix"));
                Assert.Equal("Damon Baird", result.First());
                Assert.Equal("Marcus Fenix", result.Last());
            }
        }

        [ConditionalFact]
        public virtual void Subquery_containing_join_projecting_main_from_clause_gets_lifted()
        {
            using (var ctx = CreateContext())
            {
                var query = from g in (from gear in ctx.Gears
                                       join tag in ctx.Tags on gear.Nickname equals tag.GearNickName
                                       orderby tag.Note
                                       select gear).AsTracking()
                            orderby g.Nickname
                            select g.Nickname;

                var result = query.ToList();

                Assert.Equal(5, result.Count);
                Assert.Equal("Baird", result[0]);
                Assert.Equal("Cole Train", result[1]);
                Assert.Equal("Dom", result[2]);
                Assert.Equal("Marcus", result[3]);
                Assert.Equal("Paduk", result[4]);
            }
        }

        [ConditionalFact]
        public virtual void Subquery_containing_left_join_projecting_main_from_clause_gets_lifted()
        {
            using (var ctx = CreateContext())
            {
                var query = from g in (from gear in ctx.Gears
                                       join tag in ctx.Tags on gear.Nickname equals tag.GearNickName into grouping
                                       from tag in grouping.DefaultIfEmpty()
                                       orderby gear.Rank
                                       select gear).AsTracking()
                            orderby g.Nickname
                            select g.Nickname;

                var result = query.ToList();

                Assert.Equal(5, result.Count);
                Assert.Equal("Baird", result[0]);
                Assert.Equal("Cole Train", result[1]);
                Assert.Equal("Dom", result[2]);
                Assert.Equal("Marcus", result[3]);
                Assert.Equal("Paduk", result[4]);
            }
        }

        [ConditionalFact]
        public virtual void Subquery_containing_join_gets_lifted_clashing_names()
        {
            using (var ctx = CreateContext())
            {
                var query = from gear in (from gear in ctx.Gears
                                          join tag in ctx.Tags on gear.Nickname equals tag.GearNickName
                                          orderby tag.Note
                                          where tag.GearNickName != "Cole Train"
                                          select gear).AsTracking()
                            join tag in ctx.Tags on gear.Nickname equals tag.GearNickName
                            orderby gear.Nickname, tag.Id
                            select gear.Nickname;

                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.Equal("Baird", result[0]);
                Assert.Equal("Dom", result[1]);
                Assert.Equal("Marcus", result[2]);
                Assert.Equal("Paduk", result[3]);
            }
        }

        [ConditionalFact]
        public virtual void Subquery_created_by_include_gets_lifted_nested()
        {
            using (var ctx = CreateContext())
            {
                var query = from gear in ctx.Gears.OrderBy(g => g.Rank).Where(g => g.Weapons.Any()).Include(g => g.CityOfBirth)
                            where !gear.HasSoulPatch
                            orderby gear.Nickname
                            select gear;

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal("Augustus Cole", result[0].FullName);
                Assert.Equal("Hanover", result[0].CityOfBirth.Name);
                Assert.Equal("Dominic Santiago", result[1].FullName);
                Assert.Equal("Ephyra", result[1].CityOfBirth.Name);
                Assert.Equal("Garron Paduk", result[2].FullName);
                Assert.Equal("Unknown", result[2].CityOfBirth.Name);
            }
        }

        [ConditionalFact]
        public virtual void Subquery_is_not_lifted_from_additional_from_clause()
        {
            using (var ctx = CreateContext())
            {
                var query = from g1 in ctx.Gears
                            from g2 in ctx.Gears.OrderBy(g => g.Rank).Include(g => g.Tag)
                            orderby g1.FullName
                            where g1.HasSoulPatch && !g2.HasSoulPatch
                            select new { Name1 = g1.FullName, Name2 = g2.FullName };

                var result = query.ToList();

                Assert.Equal(6, result.Count);
                Assert.True(result.All(r => r.Name1 == "Damon Baird" || r.Name1 == "Marcus Fenix"));
                Assert.True(result.All(r => r.Name2 == "Augustus Cole" || r.Name2 == "Garron Paduk" || r.Name2 == "Dominic Santiago"));
            }
        }

        [ConditionalFact]
        public virtual void Subquery_with_result_operator_is_not_lifted()
        {
            using (var ctx = CreateContext())
            {
                var query = from g in ctx.Gears.Where(g => !g.HasSoulPatch).OrderBy(g => g.FullName).Take(2).AsTracking()
                            orderby g.Rank
                            select g.FullName;

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Augustus Cole", result[0]);
                Assert.Equal("Dominic Santiago", result[1]);
            }
        }

        [ConditionalFact]
        public virtual void Select_length_of_string_property()
        {
            using (var ctx = CreateContext())
            {
                var query = from w in ctx.Weapons
                            select new { w.Name, w.Name.Length };

                var result = query.ToList();
                foreach (var r in result)
                {
                    Assert.Equal(r.Name.Length, r.Length);
                }
            }
        }

        [ConditionalFact]
        public virtual void Client_method_on_collection_navigation_in_predicate()
        {
            using (var ctx = CreateContext())
            {
                var query = from g in ctx.Gears
                            where g.HasSoulPatch && FavoriteWeapon(g.Weapons).Name == "Marcus' Lancer"
                            select g.Nickname;

                var result = query.ToList();

                Assert.Equal(1, result.Count);
                Assert.Equal("Marcus", result[0]);
            }
        }

        [ConditionalFact]
        public virtual void Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property()
        {
            using (var ctx = CreateContext())
            {
                var query = from g in ctx.Gears
                            where !g.HasSoulPatch && FavoriteWeapon(EF.Property<List<Weapon>>(g, "Weapons")).Name == "Cole's Gnasher"
                            select g.Nickname;

                var result = query.ToList();

                Assert.Equal(1, result.Count);
                Assert.Equal("Cole Train", result[0]);
            }
        }

        [ConditionalFact]
        public virtual void Client_method_on_collection_navigation_in_order_by()
        {
            using (var ctx = CreateContext())
            {
                var query = from g in ctx.Gears
                            where !g.HasSoulPatch
                            orderby FavoriteWeapon(g.Weapons).Name descending
                            select g.Nickname;

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.Equal("Paduk", result[0]);
                Assert.Equal("Dom", result[1]);
                Assert.Equal("Cole Train", result[2]);
            }
        }

        [ConditionalFact]
        public virtual void Client_method_on_collection_navigation_in_additional_from_clause()
        {
            using (var ctx = CreateContext())
            {
                var query = from g in ctx.Gears.OfType<Officer>()
                            from v in Veterans(g.Reports)
                            orderby g.Nickname, v.Nickname
                            select new { g = g.Nickname, v = v.Nickname };

                var result = query.ToList();

                Assert.Equal(3, result.Count);
                Assert.True(result.Select(r => r.g).All(g => g == "Marcus"));
                Assert.Equal("Baird", result[0].v);
                Assert.Equal("Cole Train", result[1].v);
                Assert.Equal("Dom", result[2].v);
            }
        }

        [ConditionalFact]
        public virtual void Client_method_on_collection_navigation_in_outer_join_key()
        {
            using (var ctx = CreateContext())
            {
                var query = from o in ctx.Gears.OfType<Officer>()
                            join g in ctx.Gears on FavoriteWeapon(o.Weapons).Name equals FavoriteWeapon(g.Weapons).Name
                            where o.HasSoulPatch
                            orderby o.Nickname, g.Nickname
                            select new { o = o.Nickname, g = g.Nickname };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Baird", result[0].o);
                Assert.Equal("Baird", result[0].g);
                Assert.Equal("Marcus", result[1].o);
                Assert.Equal("Marcus", result[1].g);
            }
        }

        private static Weapon FavoriteWeapon(IEnumerable<Weapon> weapons)
            => weapons.OrderBy(w => w.Id).FirstOrDefault();

        private static IEnumerable<Gear> Veterans(IEnumerable<Gear> gears)
        {
            return gears.Where(g => g.Nickname == "Marcus" || g.Nickname == "Dom" || g.Nickname == "Cole Train" || g.Nickname == "Baird");
        }

        [ConditionalFact]
        public virtual void Member_access_on_derived_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby ((LocustHorde)f).Name
                            select new { ((LocustHorde)f).Name, ((LocustHorde)f).Eradicated };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.True(result[0].Eradicated);
                Assert.Equal("Swarm", result[1].Name);
                Assert.False(result[1].Eradicated);
            }
        }

        [ConditionalFact]
        public virtual void Member_access_on_derived_materialized_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby f.Name
                            select new { f, ((LocustHorde)f).Eradicated };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].f.Name);
                Assert.True(result[0].Eradicated);
                Assert.Equal("Swarm", result[1].f.Name);
                Assert.False(result[1].Eradicated);
            }
        }

        [ConditionalFact]
        public virtual void Member_access_on_derived_entity_using_cast_and_let()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            let horde = (LocustHorde)f
                            orderby horde.Name
                            select new { horde.Name, horde.Eradicated };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.True(result[0].Eradicated);
                Assert.Equal("Swarm", result[1].Name);
                Assert.False(result[1].Eradicated);
            }
        }

        [ConditionalFact]
        public virtual void Property_access_on_derived_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            let horde = (LocustHorde)f
                            orderby f.Name
                            select new { Name = EF.Property<string>(horde, "Name"), Eradicated = EF.Property<bool>((LocustHorde)f, "Eradicated") };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.True(result[0].Eradicated);
                Assert.Equal("Swarm", result[1].Name);
                Assert.False(result[1].Eradicated);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_access_on_derived_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby f.Name
                            select new { f.Name, Threat = ((LocustHorde)f).Commander.ThreatLevel };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal(5, result[0].Threat);
                Assert.Equal("Swarm", result[1].Name);
                Assert.Equal(0, result[1].Threat);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_access_on_derived_materialized_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby f.Name
                            select new { f, f.Name, Threat = ((LocustHorde)f).Commander.ThreatLevel };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal("Locust", result[0].f.Name);
                Assert.Equal(5, result[0].Threat);
                Assert.Equal("Swarm", result[1].Name);
                Assert.Equal("Swarm", result[1].f.Name);
                Assert.Equal(0, result[1].Threat);
            }
        }


        [ConditionalFact]
        public virtual void Navigation_access_via_EFProperty_on_derived_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby f.Name
                            select new { f.Name, Threat = EF.Property<LocustCommander>((LocustHorde)f, "Commander").ThreatLevel };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal(5, result[0].Threat);
                Assert.Equal("Swarm", result[1].Name);
                Assert.Equal(0, result[1].Threat);
            }
        }

        [ConditionalFact]
        public virtual void Navigation_access_fk_on_derived_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby f.Name
                            select new { f.Name, CommanderName = ((LocustHorde)f).Commander.Name };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal("Queen Myrrah", result[0].CommanderName);
                Assert.Equal("Swarm", result[1].Name);
                Assert.Equal("Unknown", result[1].CommanderName);
            }
        }

        [ConditionalFact]
        public virtual void Collection_navigation_access_on_derived_entity_using_cast()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            where f is LocustHorde
                            orderby f.Name
                            select new { f.Name, LeadersCount = ((LocustHorde)f).Leaders.Count };

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal(4, result[0].LeadersCount);
                Assert.Equal("Swarm", result[1].Name);
                Assert.Equal(1, result[1].LeadersCount);
            }
        }

        [ConditionalFact]
        public virtual void Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions.Where(f => f is LocustHorde)
                            from l in ((LocustHorde)f).Leaders
                            orderby l.Name
                            select new { f.Name, LeaderName = l.Name };

                var result = query.ToList();

                Assert.Equal(5, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal("Locust", result[1].Name);
                Assert.Equal("Locust", result[2].Name);
                Assert.Equal("Locust", result[3].Name);
                Assert.Equal("Swarm", result[4].Name);
                Assert.Equal("General Karn", result[0].LeaderName);
                Assert.Equal("General RAAM", result[1].LeaderName);
                Assert.Equal("High Priest Skorge", result[2].LeaderName);
                Assert.Equal("Queen Myrrah", result[3].LeaderName);
                Assert.Equal("The Speaker", result[4].LeaderName);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_derived_entity_using_OfType()
        {
            using (var ctx = CreateContext())
            {
                var query = from lh in ctx.Factions.OfType<LocustHorde>().Include(h => h.Commander).Include(h => h.Leaders)
                            orderby lh.Name
                            select lh;

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Queen Myrrah", result[0].Commander.Name);
                Assert.Equal(4, result[0].Leaders.Count);
                Assert.Equal("Unknown", result[1].Commander.Name);
                Assert.Equal(1, result[1].Leaders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_derived_entity_using_subquery_with_cast()
        {
            using (var ctx = CreateContext())
            {

                var query = from lh in (from f in ctx.Factions
                                        where f is LocustHorde
                                        select (LocustHorde)f).Include(h => h.Commander).Include(h => h.Leaders)
                            orderby lh.Name
                            select lh;

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Queen Myrrah", result[0].Commander.Name);
                Assert.Equal(4, result[0].Leaders.Count);
                Assert.Equal("Unknown", result[1].Commander.Name);
                Assert.Equal(1, result[1].Leaders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_derived_entity_using_subquery_with_cast_AsNoTracking()
        {
            using (var ctx = CreateContext())
            {

                var query = from lh in (from f in ctx.Factions
                                        where f is LocustHorde
                                        select (LocustHorde)f).AsNoTracking().Include(h => h.Commander).Include(h => h.Leaders)
                            orderby lh.Name
                            select lh;

                var result = query.ToList();

                Assert.Equal(2, result.Count);
                Assert.Equal("Queen Myrrah", result[0].Commander.Name);
                Assert.Equal(4, result[0].Leaders.Count);
                Assert.Equal("Unknown", result[1].Commander.Name);
                Assert.Equal(1, result[1].Leaders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Include_on_derived_entity_using_subquery_with_cast_cross_product_base_entity()
        {
            using (var ctx = CreateContext())
            {

                var query = from lh in (from f2 in ctx.Factions
                                        where f2 is LocustHorde
                                        select (LocustHorde)f2).Include(h => h.Commander).Include(h => h.Leaders)
                            from f in ctx.Factions.Include(ff => ff.Capital)
                            orderby lh.Name, f.Name
                            select new { lh, f };

                var result = query.ToList();

                Assert.Equal(4, result.Count);
                Assert.Equal("Queen Myrrah", result[0].lh.Commander.Name);
                Assert.Equal(4, result[0].lh.Leaders.Count);
                Assert.Equal("Unknown", result[2].lh.Commander.Name);
                Assert.Equal(1, result[2].lh.Leaders.Count);
            }
        }

        [ConditionalFact]
        public virtual void Distinct_on_subquery_doesnt_get_lifted()
        {
            using (var ctx = CreateContext())
            {
                var query = from g in (from ig in ctx.Gears
                                       select ig).Distinct()
                            select g.HasSoulPatch;

                var result = query.ToList();
                Assert.Equal(5, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert()
        {
            using (var ctx = CreateContext())
            {
                var query = from lh in (from f in ctx.Factions
                                        select f).Cast<LocustHorde>()
                            select lh.Eradicated;

                var result = query.ToList();
                Assert.Equal(2, result.Count);
            }
        }

        [ConditionalFact]
        public virtual void Comparing_two_collection_navigations_composite_key()
        {
            using (var ctx = CreateContext())
            {
                var query = from g1 in ctx.Gears
                            from g2 in ctx.Gears
                            // ReSharper disable once PossibleUnintendedReferenceComparison
                            where g1.Weapons == g2.Weapons
                            orderby g1.Nickname
                            select new { Nickname1 = g1.Nickname, Nickname2 = g2.Nickname };

                var result = query.ToList();
                Assert.Equal(5, result.Count);
                Assert.Equal(5, result.Count(r => r.Nickname1 == r.Nickname2));
            }
        }

        [ConditionalFact]
        public virtual void Comparing_two_collection_navigations_inheritance()
        {
            using (var ctx = CreateContext())
            {
                var query = from f in ctx.Factions
                            from o in ctx.Gears.OfType<Officer>()
                            where f is LocustHorde && o.HasSoulPatch
                            // ReSharper disable once PossibleUnintendedReferenceComparison
                            where ((LocustHorde)f).Commander.DefeatedBy.Weapons == o.Weapons
                            select new { f.Name, o.Nickname };

                var result = query.ToList();
                Assert.Equal(1, result.Count);
                Assert.Equal("Locust", result[0].Name);
                Assert.Equal("Marcus", result[0].Nickname);
            }
        }

        [ConditionalFact(Skip = "issue #8375")]
        public virtual void Comparing_entities_using_Equals_inheritance()
        {
            using (var ctx = CreateContext())
            {
                var query = from g in ctx.Gears
                            from o in ctx.Gears.OfType<Officer>()
                            where g.Equals(o)
                            orderby g.Nickname
                            select new { Nickname1 = g.Nickname, Nickname2 = o.Nickname };

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.Equal("Baird", result[0].Nickname1);
                Assert.Equal("Baird", result[0].Nickname2);
                Assert.Equal("Marcus", result[1].Nickname1);
                Assert.Equal("Marcus", result[1].Nickname2);
            }
        }

        [ConditionalFact]
        public virtual void Contains_on_nullable_array_produces_correct_sql()
        {
            using (var context = CreateContext())
            {
                var cities = new[] { "Ephyra", null };
                var query = context.Gears.Where(g => g.SquadId < 2 && cities.Contains(g.AssignedCity.Name)).ToList();

                Assert.Equal(2, query.Count);
            }
        }

        [ConditionalFact]
        public virtual void Optional_navigation_with_collection_composite_key()
        {
            using (var context = CreateContext())
            {
                var query = context.Tags.Where(t => t.Gear is Officer && ((Officer)t.Gear).Reports.Count(r => r.Nickname == "Dom") > 0);
                var result = query.ToList();

                Assert.Equal(1, result.Count);
                Assert.Equal("Marcus's Tag", result[0].Note);
            }
        }

        [ConditionalFact]
        public virtual void Select_null_conditional_with_inheritance()
        {
            using (var context = CreateContext())
            {
                var query = context.Factions
                    .Where(f => f is LocustHorde)
                    .Select(f => EF.Property<string>((LocustHorde)f, "CommanderName") != null ? ((LocustHorde)f).CommanderName : null);

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.True(result.Contains("Queen Myrrah"));
                Assert.True(result.Contains("Unknown"));
            }
        }

        [ConditionalFact]
        public virtual void Select_null_conditional_with_inheritance_negative()
        {
            using (var context = CreateContext())
            {
                var query = context.Factions
                    .Where(f => f is LocustHorde)
                    .Select(f => EF.Property<string>((LocustHorde)f, "CommanderName") != null ? ((LocustHorde)f).Eradicated : null);

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.True(result.Contains(true));
                Assert.True(result.Contains(false));
            }
        }

        [ConditionalFact]
        public virtual void Project_collection_navigation_with_inheritance1()
        {
            using (var context = CreateContext())
            {
                var query = context.Factions.OfType<LocustHorde>()
                    .Select(h => new
                    {
                        h.Id,
                        Leaders = EF.Property<ICollection<LocustLeader>>(h.Commander.CommandingFaction, "Leaders")
                    });

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.Equal(1, result.Count(r => r.Id == 1 && r.Leaders.Count == 4));
                Assert.Equal(1, result.Count(r => r.Id == 2 && r.Leaders.Count == 1));
            }
        }

        [ConditionalFact]
        public virtual void Project_collection_navigation_with_inheritance2()
        {
            using (var context = CreateContext())
            {
                var query = context.Factions.OfType<LocustHorde>()
                    .Select(h => new
                    {
                        h.Id,
                        Gears = EF.Property<ICollection<Gear>>((Officer)h.Commander.DefeatedBy, "Reports")
                    });

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.Equal(1, result.Count(r => r.Id == 1 && r.Gears.Count == 3));
                Assert.Equal(1, result.Count(r => r.Id == 2 && r.Gears.Count == 0));
            }
        }

        [ConditionalFact]
        public virtual void Project_collection_navigation_with_inheritance3()
        {
            using (var context = CreateContext())
            {
                var query = context.Factions
                    .Where(f => f is LocustHorde)
                    .Select(f => new
                    {
                        f.Id,
                        Gears = EF.Property<ICollection<Gear>>((Officer)((LocustHorde)f).Commander.DefeatedBy, "Reports")
                    });

                var result = query.ToList();
                Assert.Equal(2, result.Count);
                Assert.Equal(1, result.Count(r => r.Id == 1 && r.Gears.Count == 3));
                Assert.Equal(1, result.Count(r => r.Id == 2 && r.Gears.Count == 0));
            }
        }

        [ConditionalFact]
        public virtual void Enum_ToString_is_client_eval()
        {
            using (var context = CreateContext())
            {
                var query = context.Gears
                    .OrderBy(g => g.SquadId)
                    .ThenBy(g => g.Nickname)
                    .Select(g => g.Rank.ToString())
                    .Take(1)
                    .ToList();

                var result = Assert.Single(query);
                Assert.Equal("Corporal", result);
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
