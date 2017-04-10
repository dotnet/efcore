// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.GearsOfWarModel
{
    public class GearsOfWarModelInitializer
    {
        public static void Seed(GearsOfWarContext context)
        {
            var deltaSquad = new Squad
            {
                Name = "Delta",
                Members = new List<Gear>()
            };

            context.Squads.Add(deltaSquad);

            var kiloSquad = new Squad
            {
                Name = "Kilo",
                Members = new List<Gear>()
            };

            context.Squads.Add(kiloSquad);

            var lightmassOffensive = new Mission
            {
                CodeName = "Lightmass Offensive",
                Timeline = new DateTimeOffset(2, 1, 2, 10, 0, 0, new TimeSpan())
            };

            var hollowStorm = new Mission
            {
                CodeName = "Lightmass Offensive",
                Timeline = new DateTimeOffset(2, 3, 1, 8, 0, 0, new TimeSpan())
            };

            var halvoBay = new Mission
            {
                CodeName = "Halvo Bay defense",
                Timeline = new DateTimeOffset(10, 5, 3, 12, 0, 0, new TimeSpan())
            };

            var lightMassDelta = new SquadMission
            {
                Mission = lightmassOffensive,
                Squad = deltaSquad,
            };

            var hollowStormDelta = new SquadMission
            {
                Mission = hollowStorm,
                Squad = deltaSquad,
            };

            var halvoBayKilo = new SquadMission
            {
                Mission = halvoBay,
                Squad = kiloSquad,
            };

            context.Missions.AddRange(lightmassOffensive, hollowStorm, halvoBay);
            context.SquadMissions.AddRange(lightMassDelta, hollowStormDelta, halvoBayKilo);

            var jacinto = new City
            {
                Location = "Jacinto's location",
                Name = "Jacinto"
            };

            var ephyra = new City
            {
                Location = "Ephyra's location",
                Name = "Ephyra"
            };

            var hanover = new City
            {
                Location = "Hanover's location",
                Name = "Hanover"
            };

            var unknown = new City
            {
                Location = "Unknown",
                Name = "Unknown"
            };

            var marcusLancer = new Weapon
            {
                Id = 1,
                Name = "Marcus' Lancer",
                AmmunitionType = AmmunitionType.Cartridge,
                IsAutomatic = true
            };

            var marcusGnasher = new Weapon
            {
                Id = 2,
                Name = "Marcus' Gnasher",
                AmmunitionType = AmmunitionType.Shell,
                IsAutomatic = false,
                SynergyWith = marcusLancer
            };

            var domsHammerburst = new Weapon
            {
                Id = 3,
                Name = "Dom's Hammerburst",
                AmmunitionType = AmmunitionType.Cartridge,
                IsAutomatic = false
            };

            var domsGnasher = new Weapon
            {
                Id = 4,
                Name = "Dom's Gnasher",
                AmmunitionType = AmmunitionType.Shell,
                IsAutomatic = false
            };

            var colesGnasher = new Weapon
            {
                Id = 5,
                Name = "Cole's Gnasher",
                AmmunitionType = AmmunitionType.Shell,
                IsAutomatic = false
            };

            var colesMulcher = new Weapon
            {
                Id = 6,
                Name = "Cole's Mulcher",
                AmmunitionType = AmmunitionType.Cartridge,
                IsAutomatic = true
            };

            var bairdsLancer = new Weapon
            {
                Id = 7,
                Name = "Baird's Lancer",
                AmmunitionType = AmmunitionType.Cartridge,
                IsAutomatic = true
            };

            var bairdsGnasher = new Weapon
            {
                Id = 8,
                Name = "Baird's Gnasher",
                AmmunitionType = AmmunitionType.Shell,
                IsAutomatic = false
            };

            var paduksMarkza = new Weapon
            {
                Id = 9,
                Name = "Paduk's Markza",
                AmmunitionType = AmmunitionType.Cartridge,
                IsAutomatic = false
            };

            var maulersFlail = new Weapon
            {
                Id = 10,
                Name = "Mauler's Flail",
                IsAutomatic = false
            };

            context.Weapons.Add(marcusLancer);
            context.Weapons.Add(marcusGnasher);
            context.Weapons.Add(domsHammerburst);
            context.Weapons.Add(domsGnasher);
            context.Weapons.Add(colesGnasher);
            context.Weapons.Add(colesMulcher);
            context.Weapons.Add(bairdsLancer);
            context.Weapons.Add(bairdsGnasher);
            context.Weapons.Add(paduksMarkza);
            context.Weapons.Add(maulersFlail);

            var marcusTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Marcus's Tag"
            };

            var domsTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Dom's Tag"
            };

            var colesTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Cole's Tag"
            };

            var bairdsTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Bairds's Tag"
            };

            var paduksTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "Paduk's Tag"
            };

            var kiaTag = new CogTag
            {
                Id = Guid.NewGuid(),
                Note = "K.I.A."
            };

            context.Tags.Add(kiaTag);

            var dom = new Gear
            {
                Nickname = "Dom",
                FullName = "Dominic Santiago",
                HasSoulPatch = false,
                SquadId = deltaSquad.Id,
                Rank = MilitaryRank.Corporal,
                AssignedCity = ephyra,
                CityOfBirth = ephyra,
                Tag = domsTag,
                Weapons = new List<Weapon> { domsHammerburst, domsGnasher }
            };

            var cole = new Gear
            {
                Nickname = "Cole Train",
                FullName = "Augustus Cole",
                HasSoulPatch = false,
                SquadId = deltaSquad.Id,
                Rank = MilitaryRank.Private,
                CityOfBirth = hanover,
                AssignedCity = jacinto,
                Tag = colesTag,
                Weapons = new List<Weapon> { colesGnasher, colesMulcher }
            };

            var paduk = new Gear
            {
                Nickname = "Paduk",
                FullName = "Garron Paduk",
                HasSoulPatch = false,
                SquadId = kiloSquad.Id,
                Rank = MilitaryRank.Private,
                CityOfBirth = unknown,
                Tag = paduksTag,
                Weapons = new List<Weapon> { paduksMarkza }
            };

            var baird = new Officer
            {
                Nickname = "Baird",
                FullName = "Damon Baird",
                HasSoulPatch = true,
                SquadId = deltaSquad.Id,
                Rank = MilitaryRank.Corporal,
                CityOfBirth = unknown,
                AssignedCity = jacinto,
                Tag = bairdsTag,
                Reports = new List<Gear> { paduk },
                Weapons = new List<Weapon> { bairdsLancer, bairdsGnasher }
            };

            var marcus = new Officer
            {
                Nickname = "Marcus",
                FullName = "Marcus Fenix",
                HasSoulPatch = true,
                SquadId = deltaSquad.Id,
                Rank = MilitaryRank.Sergeant,
                CityOfBirth = jacinto,
                Tag = marcusTag,
                Reports = new List<Gear> { dom, cole, baird },
                Weapons = new List<Weapon> { marcusLancer, marcusGnasher }
            };

            context.Gears.Add(marcus);
            context.Gears.Add(dom);
            context.Gears.Add(cole);
            context.Gears.Add(baird);
            context.Gears.Add(paduk);

            context.SaveChanges();
        }
    }
}
