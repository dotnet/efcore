// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel
{
    public class GearsOfWarData : ISetSource
    {
        public IReadOnlyList<City> Cities { get; }
        public IReadOnlyList<CogTag> Tags { get; }
        public IReadOnlyList<Faction> Factions { get; }
        public IReadOnlyList<Gear> Gears { get; }
        public IReadOnlyList<Mission> Missions { get; }
        public IReadOnlyList<Squad> Squads { get; }
        public IReadOnlyList<SquadMission> SquadMissions { get; }
        public IReadOnlyList<Weapon> Weapons { get; }
        public IReadOnlyList<LocustLeader> LocustLeaders { get; }
        public IReadOnlyList<LocustHighCommand> LocustHighCommands { get; }

        public GearsOfWarData()
        {
            Squads = CreateSquads();
            Missions = CreateMissions();
            SquadMissions = CreateSquadMissions();
            Cities = CreateCities();
            Weapons = CreateWeapons();
            Tags = CreateTags();
            Gears = CreateGears();
            LocustLeaders = CreateLocustLeaders();
            Factions = CreateFactions();
            LocustHighCommands = CreateHighCommands();

            WireUp(Squads, Missions, SquadMissions, Cities, Weapons, Tags, Gears, LocustLeaders, Factions, LocustHighCommands);
            WireUp2(LocustLeaders, Factions);
        }

        public virtual IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(City))
            {
                return (IQueryable<TEntity>)Cities.AsQueryable();
            }

            if (typeof(TEntity) == typeof(CogTag))
            {
                return (IQueryable<TEntity>)Tags.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Faction))
            {
                return (IQueryable<TEntity>)Factions.AsQueryable();
            }

            if (typeof(TEntity) == typeof(LocustHorde))
            {
                return (IQueryable<TEntity>)Factions.OfType<LocustHorde>().AsQueryable();
            }

            if (typeof(TEntity) == typeof(Gear))
            {
                return (IQueryable<TEntity>)Gears.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Officer))
            {
                return (IQueryable<TEntity>)Gears.OfType<Officer>().AsQueryable();
            }

            if (typeof(TEntity) == typeof(Mission))
            {
                return (IQueryable<TEntity>)Missions.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Squad))
            {
                return (IQueryable<TEntity>)Squads.AsQueryable();
            }

            if (typeof(TEntity) == typeof(SquadMission))
            {
                return (IQueryable<TEntity>)SquadMissions.AsQueryable();
            }

            if (typeof(TEntity) == typeof(Weapon))
            {
                return (IQueryable<TEntity>)Weapons.AsQueryable();
            }

            if (typeof(TEntity) == typeof(LocustLeader))
            {
                return (IQueryable<TEntity>)LocustLeaders.AsQueryable();
            }

            if (typeof(TEntity) == typeof(LocustHighCommand))
            {
                return (IQueryable<TEntity>)LocustHighCommands.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        public static IReadOnlyList<Squad> CreateSquads()
            => new List<Squad>
            {
                new Squad { Id = 1, Name = "Delta", Banner = new byte[] { 0x00, 0x01 }, Banner5 = new byte[] { 0x04, 0x05, 0x06, 0x07, 0x08 } },
                new Squad { Id = 2, Name = "Kilo", Banner = new byte[] { 0x02, 0x03 }, Banner5 = new byte[] { 0x04, 0x05, 0x06, 0x07, 0x08 } }
            };

        public static IReadOnlyList<Mission> CreateMissions()
            => new List<Mission>
            {
                new Mission
                {
                    Id = 1,
                    CodeName = "Lightmass Offensive",
                    Rating = 2.1,
                    Timeline = new DateTimeOffset(599898024001234567, new TimeSpan(1, 30, 0))
                },
                new Mission
                {
                    Id = 2,
                    CodeName = "Hollow Storm",
                    Rating = 4.2,
                    Timeline = new DateTimeOffset(2, 3, 1, 8, 0, 0, new TimeSpan(-5, 0, 0))
                },
                new Mission
                {
                    Id = 3,
                    CodeName = "Halvo Bay defense",
                    Rating = null,
                    Timeline = new DateTimeOffset(10, 5, 3, 12, 0, 0, new TimeSpan())
                }
            };

        public static IReadOnlyList<SquadMission> CreateSquadMissions()
            => new List<SquadMission>
            {
                new SquadMission(),
                new SquadMission(),
                new SquadMission()
            };

        public static IReadOnlyList<City> CreateCities()
        {
            var jacinto = new City
            {
                Location = "Jacinto's location",
                Name = "Jacinto",
            };
            jacinto["Nation"] = "Tyrus";

            var ephyra = new City
            {
                Location = "Ephyra's location",
                Name = "Ephyra",
            };
            ephyra["Nation"] = "Tyrus";

            var hanover = new City { Location = "Hanover's location", Name = "Hanover" };

            var unknown = new City { Location = "Unknown", Name = "Unknown" };

            var cities = new List<City>
            {
                jacinto,
                ephyra,
                hanover,
                unknown
            };
            return cities;
        }

        public static IReadOnlyList<Weapon> CreateWeapons()
            => new List<Weapon>
            {
                new Weapon
                {
                    Id = 1,
                    Name = "Marcus' Lancer",
                    AmmunitionType = AmmunitionType.Cartridge,
                    IsAutomatic = true
                },
                new Weapon
                {
                    Id = 2,
                    Name = "Marcus' Gnasher",
                    AmmunitionType = AmmunitionType.Shell,
                    IsAutomatic = false
                },
                new Weapon
                {
                    Id = 3,
                    Name = "Dom's Hammerburst",
                    AmmunitionType = AmmunitionType.Cartridge,
                    IsAutomatic = false
                },
                new Weapon
                {
                    Id = 4,
                    Name = "Dom's Gnasher",
                    AmmunitionType = AmmunitionType.Shell,
                    IsAutomatic = false
                },
                new Weapon
                {
                    Id = 5,
                    Name = "Cole's Gnasher",
                    AmmunitionType = AmmunitionType.Shell,
                    IsAutomatic = false
                },
                new Weapon
                {
                    Id = 6,
                    Name = "Cole's Mulcher",
                    AmmunitionType = AmmunitionType.Cartridge,
                    IsAutomatic = true
                },
                new Weapon
                {
                    Id = 7,
                    Name = "Baird's Lancer",
                    AmmunitionType = AmmunitionType.Cartridge,
                    IsAutomatic = true
                },
                new Weapon
                {
                    Id = 8,
                    Name = "Baird's Gnasher",
                    AmmunitionType = AmmunitionType.Shell,
                    IsAutomatic = false
                },
                new Weapon
                {
                    Id = 9,
                    Name = "Paduk's Markza",
                    AmmunitionType = AmmunitionType.Cartridge,
                    IsAutomatic = false
                },
                new Weapon
                {
                    Id = 10,
                    Name = "Mauler's Flail",
                    IsAutomatic = false
                }
            };

        public static IReadOnlyList<CogTag> CreateTags()
            => new List<CogTag>
            {
                new CogTag { Id = Guid.Parse("DF36F493-463F-4123-83F9-6B135DEEB7BA"), Note = "Dom's Tag" },
                new CogTag { Id = Guid.Parse("A8AD98F9-E023-4E2A-9A70-C2728455BD34"), Note = "Cole's Tag" },
                new CogTag { Id = Guid.Parse("A7BE028A-0CF2-448F-AB55-CE8BC5D8CF69"), Note = "Paduk's Tag" },
                new CogTag { Id = Guid.Parse("70534E05-782C-4052-8720-C2C54481CE5F"), Note = "Baird's Tag" },
                new CogTag { Id = Guid.Parse("34C8D86E-A4AC-4BE5-827F-584DDA348A07"), Note = "Marcus' Tag" },
                new CogTag { Id = Guid.Parse("B39A6FBA-9026-4D69-828E-FD7068673E57"), Note = "K.I.A." }
            };

        public static IReadOnlyList<Gear> CreateGears()
            => new List<Gear>
            {
                new Gear
                {
                    Nickname = "Dom",
                    FullName = "Dominic Santiago",
                    HasSoulPatch = false,
                    SquadId = 1,
                    Rank = MilitaryRank.Corporal,
                    CityOfBirthName = "Ephyra",
                    LeaderNickname = "Marcus",
                    LeaderSquadId = 1
                },
                new Gear
                {
                    Nickname = "Cole Train",
                    FullName = "Augustus Cole",
                    HasSoulPatch = false,
                    SquadId = 1,
                    Rank = MilitaryRank.Private,
                    CityOfBirthName = "Hanover",
                    LeaderNickname = "Marcus",
                    LeaderSquadId = 1
                },
                new Gear
                {
                    Nickname = "Paduk",
                    FullName = "Garron Paduk",
                    HasSoulPatch = false,
                    SquadId = 2,
                    Rank = MilitaryRank.Private,
                    CityOfBirthName = "Unknown",
                    LeaderNickname = "Baird",
                    LeaderSquadId = 1
                },
                new Officer
                {
                    Nickname = "Baird",
                    FullName = "Damon Baird",
                    HasSoulPatch = true,
                    SquadId = 1,
                    Rank = MilitaryRank.Corporal,
                    CityOfBirthName = "Unknown",
                    LeaderNickname = "Marcus",
                    LeaderSquadId = 1
                },
                new Officer
                {
                    Nickname = "Marcus",
                    FullName = "Marcus Fenix",
                    HasSoulPatch = true,
                    SquadId = 1,
                    Rank = MilitaryRank.Sergeant,
                    CityOfBirthName = "Jacinto"
                }
            };

        public static IReadOnlyList<LocustLeader> CreateLocustLeaders()
            => new List<LocustLeader>
            {
                new LocustLeader { Name = "General Karn", ThreatLevel = 3 },
                new LocustLeader { Name = "General RAAM", ThreatLevel = 4 },
                new LocustLeader { Name = "High Priest Skorge", ThreatLevel = 1 },
                new LocustCommander { Name = "Queen Myrrah", ThreatLevel = 5 },
                new LocustLeader { Name = "The Speaker", ThreatLevel = 3 },
                new LocustCommander { Name = "Unknown", ThreatLevel = 0 }
            };

        public static IReadOnlyList<Faction> CreateFactions()
            => new List<Faction>
            {
                new LocustHorde
                {
                    Id = 1,
                    Name = "Locust",
                    Eradicated = true,
                    CommanderName = "Queen Myrrah"
                },
                new LocustHorde
                {
                    Id = 2,
                    Name = "Swarm",
                    Eradicated = false,
                    CommanderName = "Unknown"
                }
            };

        public static IReadOnlyList<LocustHighCommand> CreateHighCommands()
            => new List<LocustHighCommand>
            {
                new LocustHighCommand
                {
                    Id = 1,
                    Name = "Locust Main Command",
                    IsOperational = true
                }
            };

        public static void WireUp(
            IReadOnlyList<Squad> squads,
            IReadOnlyList<Mission> missions,
            IReadOnlyList<SquadMission> squadMissions,
            IReadOnlyList<City> cities,
            IReadOnlyList<Weapon> weapons,
            IReadOnlyList<CogTag> tags,
            IReadOnlyList<Gear> gears,
            IReadOnlyList<LocustLeader> locustLeaders,
            IReadOnlyList<Faction> factions,
            IReadOnlyList<LocustHighCommand> locustHighCommands)
        {
            squadMissions[0].Mission = missions[0];
            squadMissions[0].MissionId = missions[0].Id;
            squadMissions[0].Squad = squads[0];
            squadMissions[0].SquadId = squads[0].Id;
            squadMissions[1].Mission = missions[1];
            squadMissions[1].MissionId = missions[1].Id;
            squadMissions[1].Squad = squads[0];
            squadMissions[1].SquadId = squads[0].Id;
            squadMissions[2].Mission = missions[2];
            squadMissions[2].MissionId = missions[2].Id;
            squadMissions[2].Squad = squads[1];
            squadMissions[2].SquadId = squads[1].Id;

            missions[0].ParticipatingSquads = new List<SquadMission> { squadMissions[0] };
            missions[1].ParticipatingSquads = new List<SquadMission> { squadMissions[1] };
            missions[2].ParticipatingSquads = new List<SquadMission> { squadMissions[2] };
            squads[0].Missions = new List<SquadMission> { squadMissions[0], squadMissions[1] };
            squads[1].Missions = new List<SquadMission> { squadMissions[2] };

            squads[0].Members = new List<Gear>
            {
                gears[0],
                gears[1],
                gears[3],
                gears[4]
            };
            squads[1].Members = new List<Gear> { gears[2] };

            weapons[1].SynergyWith = weapons[0];
            weapons[1].SynergyWithId = weapons[0].Id;

            // dom
            gears[0].AssignedCity = cities[1];
            gears[0].CityOfBirth = cities[1];
            gears[0].CityOfBirthName = cities[1].Name;
            gears[0].Squad = squads[0];
            gears[0].Tag = tags[0];
            gears[0].Weapons = new List<Weapon> { weapons[2], weapons[3] };

            // cole
            gears[1].AssignedCity = cities[0];
            gears[1].CityOfBirth = cities[2];
            gears[1].CityOfBirthName = cities[2].Name;
            gears[1].Squad = squads[0];
            gears[1].Tag = tags[1];
            gears[1].Weapons = new List<Weapon> { weapons[4], weapons[5] };

            // paduk
            gears[2].AssignedCity = cities[3];
            gears[2].CityOfBirth = cities[3];
            gears[2].CityOfBirthName = cities[3].Name;
            gears[2].Squad = squads[1];
            gears[2].Tag = tags[2];
            gears[2].Weapons = new List<Weapon> { weapons[8] };

            // baird
            gears[3].AssignedCity = cities[0];
            gears[3].CityOfBirth = cities[3];
            gears[3].CityOfBirthName = cities[3].Name;
            gears[3].Squad = squads[0];
            gears[3].Tag = tags[3];
            gears[3].Weapons = new List<Weapon> { weapons[6], weapons[7] };
            ((Officer)gears[3]).Reports = new List<Gear> { gears[2] };

            // marcus
            gears[4].CityOfBirth = cities[0];
            gears[4].CityOfBirthName = cities[0].Name;
            gears[4].Squad = squads[0];
            gears[4].Tag = tags[4];
            gears[4].Weapons = new List<Weapon> { weapons[0], weapons[1] };
            ((Officer)gears[4]).Reports = new List<Gear>
            {
                gears[0],
                gears[1],
                gears[3]
            };

            cities[0].BornGears = new List<Gear> { gears[4] };
            cities[1].BornGears = new List<Gear> { gears[0] };
            cities[2].BornGears = new List<Gear> { gears[1] };
            cities[3].BornGears = new List<Gear> { gears[2], gears[3] };
            cities[0].StationedGears = new List<Gear> { gears[1], gears[3] };
            cities[1].StationedGears = new List<Gear> { gears[0] };
            cities[2].StationedGears = new List<Gear>();
            cities[3].StationedGears = new List<Gear> { gears[2] };

            weapons[0].Owner = gears[4];
            weapons[0].OwnerFullName = gears[4].FullName;
            weapons[1].Owner = gears[4];
            weapons[1].OwnerFullName = gears[4].FullName;
            weapons[2].Owner = gears[0];
            weapons[2].OwnerFullName = gears[0].FullName;
            weapons[3].Owner = gears[0];
            weapons[3].OwnerFullName = gears[0].FullName;
            weapons[4].Owner = gears[1];
            weapons[4].OwnerFullName = gears[1].FullName;
            weapons[5].Owner = gears[1];
            weapons[5].OwnerFullName = gears[1].FullName;
            weapons[6].Owner = gears[3];
            weapons[6].OwnerFullName = gears[3].FullName;
            weapons[7].Owner = gears[3];
            weapons[7].OwnerFullName = gears[3].FullName;
            weapons[8].Owner = gears[2];
            weapons[8].OwnerFullName = gears[2].FullName;

            tags[0].Gear = gears[0];
            tags[0].GearNickName = gears[0].Nickname;
            tags[0].GearSquadId = gears[0].SquadId;
            tags[1].Gear = gears[1];
            tags[1].GearNickName = gears[1].Nickname;
            tags[1].GearSquadId = gears[1].SquadId;
            tags[2].Gear = gears[2];
            tags[2].GearNickName = gears[2].Nickname;
            tags[2].GearSquadId = gears[2].SquadId;
            tags[3].Gear = gears[3];
            tags[3].GearNickName = gears[3].Nickname;
            tags[3].GearSquadId = gears[3].SquadId;
            tags[4].Gear = gears[4];
            tags[4].GearNickName = gears[4].Nickname;
            tags[4].GearSquadId = gears[4].SquadId;

            ((LocustCommander)locustLeaders[3]).DefeatedBy = gears[4];
            ((LocustCommander)locustLeaders[3]).DefeatedByNickname = gears[4].Nickname;
            ((LocustCommander)locustLeaders[3]).DefeatedBySquadId = gears[4].SquadId;

            ((LocustCommander)locustLeaders[3]).CommandingFaction = ((LocustHorde)factions[0]);
            ((LocustCommander)locustLeaders[5]).CommandingFaction = ((LocustHorde)factions[1]);

            ((LocustHorde)factions[0]).Commander = ((LocustCommander)locustLeaders[3]);
            ((LocustHorde)factions[1]).Commander = ((LocustCommander)locustLeaders[5]);

            locustHighCommands[0].Commanders = new List<LocustCommander>
            {
                (LocustCommander)locustLeaders[3], (LocustCommander)locustLeaders[5]
            };

            ((LocustCommander)locustLeaders[3]).HighCommand = locustHighCommands[0];
            ((LocustCommander)locustLeaders[3]).HighCommandId = 1;

            ((LocustCommander)locustLeaders[5]).HighCommand = locustHighCommands[0];
            ((LocustCommander)locustLeaders[5]).HighCommandId = 1;
        }

        public static void WireUp2(
            IReadOnlyList<LocustLeader> locustLeaders,
            IReadOnlyList<Faction> factions)
        {
            ((LocustHorde)factions[0]).Leaders = new List<LocustLeader>
            {
                locustLeaders[0],
                locustLeaders[1],
                locustLeaders[2],
                locustLeaders[3]
            };
            ((LocustHorde)factions[1]).Leaders = new List<LocustLeader> { locustLeaders[4], locustLeaders[5] };
        }
    }
}
