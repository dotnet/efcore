// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.FunctionalTests.TestModels.GearsOfWarModel
{
    public class GearsOfWarModelInitializer
    {
        public static async Task SeedAsync(GearsOfWarContext context)
        {
            // TODO: only delete if model has changed
            await context.Database.EnsureDeletedAsync();
            if (await context.Database.EnsureCreatedAsync())
            {
                var deltaSquad = new Squad
                {
                    Name = "Delta",
                    Members = new List<Gear>(),
                };

                await context.Squads.AddAsync(deltaSquad);
                await context.SaveChangesAsync();

                var kiloSquad = new Squad
                {
                    Name = "Kilo",
                    Members = new List<Gear>(),
                };

                await context.Squads.AddAsync(kiloSquad);
                await context.SaveChangesAsync();

                var jacinto = new City
                {
                    Location = "Jacinto's location",
                    Name = "Jacinto",
                };

                var ephyra = new City
                {
                    Location = "Ephyra's location",
                    Name = "Ephyra",
                };

                var hanover = new City
                {
                    Location = "Hanover's location",
                    Name = "Hanover",
                };

                await context.Cities.AddAsync(jacinto);
                await context.Cities.AddAsync(ephyra);
                await context.Cities.AddAsync(hanover);

                await context.SaveChangesAsync();

                var marcusLancer = new Weapon
                {
                    Name = "Marcus' Lancer",
                };

                var marcusGnasher = new Weapon
                {
                    Name = "Marcus' Gnasher",
                    SynergyWith = marcusLancer,
                };

                var domsHammerburst = new Weapon
                {
                    Name = "Dom's Hammerburst",
                };

                var domsGnasher = new Weapon
                {
                    Name = "Dom's Gnasher",
                };

                var colesGnasher = new Weapon
                {
                    Name = "Cole's Gnasher",
                };

                var colesMulcher = new Weapon
                {
                    Name = "Cole's Mulcher",
                };

                var bairdsLancer = new Weapon
                {
                    Name = "Baird's Lancer",
                };

                var bairdsGnasher = new Weapon
                {
                    Name = "Baird's Gnasher",
                };

                var paduksMarkza = new Weapon
                {
                    Name = "Paduk's Markza",
                };

                await context.Weapons.AddAsync(marcusLancer);
                await context.Weapons.AddAsync(marcusGnasher);
                await context.Weapons.AddAsync(domsHammerburst);
                await context.Weapons.AddAsync(domsGnasher);
                await context.Weapons.AddAsync(colesGnasher);
                await context.Weapons.AddAsync(colesMulcher);
                await context.Weapons.AddAsync(bairdsLancer);
                await context.Weapons.AddAsync(bairdsGnasher);
                await context.Weapons.AddAsync(paduksMarkza);
                await context.SaveChangesAsync();

                var marcusTag = new CogTag
                {
                    Id = Guid.NewGuid(),
                    Note = "Marcus's Tag",
                };

                var domsTag = new CogTag
                {
                    Id = Guid.NewGuid(),
                    Note = "Dom's Tag",
                };

                var colesTag = new CogTag
                {
                    Id = Guid.NewGuid(),
                    Note = "Cole's Tag",
                };

                var bairdsTag = new CogTag
                {
                    Id = Guid.NewGuid(),
                    Note = "Bairds's Tag",
                };

                var paduksTag = new CogTag
                {
                    Id = Guid.NewGuid(),
                    Note = "Paduk's Tag",
                };

                var kiaTag = new CogTag
                {
                    Id = Guid.NewGuid(),
                    Note = "K.I.A.",
                };

                await context.Tags.AddAsync(marcusTag);
                await context.Tags.AddAsync(domsTag);
                await context.Tags.AddAsync(colesTag);
                await context.Tags.AddAsync(bairdsTag);
                await context.Tags.AddAsync(paduksTag);
                await context.Tags.AddAsync(kiaTag);
                await context.SaveChangesAsync();

                var dom = new Gear
                {
                    Nickname = "Dom",
                    FullName = "Dominic Santiago",
                    SquadId = deltaSquad.Id,
                    Rank = MilitaryRank.Corporal,
                    CityOrBirthName = ephyra.Name,
                    Tag = domsTag,
                    Reports = new List<Gear>(),
                    Weapons = new List<Weapon> { domsHammerburst, domsGnasher }
                };

                var cole = new Gear
                {
                    Nickname = "Cole Train",
                    FullName = "Augustus Cole",
                    SquadId = deltaSquad.Id,
                    Rank = MilitaryRank.Private,
                    CityOrBirthName = hanover.Name,
                    Tag = colesTag,
                    Reports = new List<Gear>(),
                    Weapons = new List<Weapon> { colesGnasher, colesMulcher }
                };

                var paduk = new Gear
                {
                    Nickname = "Paduk",
                    FullName = "Garron Paduk",
                    SquadId = kiloSquad.Id,
                    Rank = MilitaryRank.Private,
                    Tag = paduksTag,
                    Reports = new List<Gear>(),
                    Weapons = new List<Weapon> { paduksMarkza },
                };

                var baird = new Gear
                {
                    Nickname = "Baird",
                    FullName = "Damon Baird",
                    SquadId = deltaSquad.Id,
                    Rank = MilitaryRank.Corporal,
                    Tag = bairdsTag,
                    Reports = new List<Gear>() { paduk },
                    Weapons = new List<Weapon> { bairdsLancer, bairdsGnasher }
                };

                var marcus = new Gear
                {
                    Nickname = "Marcus",
                    FullName = "Marcus Fenix",
                    SquadId = deltaSquad.Id,
                    Rank = MilitaryRank.Sergeant,
                    CityOrBirthName = jacinto.Name,
                    Tag = marcusTag,
                    Reports = new List<Gear>() { dom, cole, baird },
                    Weapons = new List<Weapon> { marcusLancer, marcusGnasher },
                };

                await context.Gears.AddAsync(marcus);
                await context.Gears.AddAsync(dom);
                await context.Gears.AddAsync(cole);
                await context.Gears.AddAsync(baird);
                await context.Gears.AddAsync(paduk);

                await context.SaveChangesAsync();
            }
        }
    }
}
