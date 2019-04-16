// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel
{
    public class F1Context : PoolableDbContext
    {
        public F1Context(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Team> Teams { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Sponsor> Sponsors { get; set; }
        public DbSet<Engine> Engines { get; set; }
        public DbSet<EngineSupplier> EngineSuppliers { get; set; }

        public static void Seed(F1Context context)
        {
            AddEntities(context);

            context.SaveChanges();
        }

        private static void AddEntities(F1Context context)
        {
            foreach (var engineSupplier in new List<EngineSupplier>
            {
                new EngineSupplier
                {
                    Name = "Mercedes"
                },
                new EngineSupplier
                {
                    Name = "Renault"
                },
                new EngineSupplier
                {
                    Name = "Ferrari"
                },
                new EngineSupplier
                {
                    Name = "Cosworth"
                }
            })
            {
                context.Add(engineSupplier);
            }

            var engineSuppliers = context.EngineSuppliers.Local;
            var mercedesEngine = new Engine
            {
                Name = "FO 108X",
                StorageLocation = new Location
                {
                    Latitude = 47.64491,
                    Longitude = -122.128101
                },
                EngineSupplier = engineSuppliers.Single(s => s.Name == "Mercedes")
            };
            var renaultEngine = new Engine
            {
                Name = "RS27-2010",
                StorageLocation = new Location
                {
                    Latitude = 47.644199,
                    Longitude = -122.127049
                },
                EngineSupplier = engineSuppliers.Single(s => s.Name == "Renault")
            };
            var ferrariEngine = new Engine
            {
                Name = "056",
                StorageLocation = new Location
                {
                    Latitude = 47.64256,
                    Longitude = -122.130609
                },
                EngineSupplier = engineSuppliers.Single(s => s.Name == "Ferrari")
            };
            var cosworthEngine = new Engine
            {
                Name = "CA2010",
                StorageLocation = new Location
                {
                    Latitude = 47.644851,
                    Longitude = -122.129781
                },
                EngineSupplier = engineSuppliers.Single(s => s.Name == "Cosworth")
            };

            foreach (var engine in new List<Engine>
            {
                mercedesEngine,
                renaultEngine,
                ferrariEngine,
                cosworthEngine
            })
            {
                context.Engines.Add(engine);
            }

            foreach (var team in new List<Team>
            {
                new Team
                {
                    Id = Team.McLaren,
                    Name = "Vodafone McLaren Mercedes",
                    Constructor = "McLaren",
                    Chassis = new Chassis
                    {
                        Name = "MP4-25"
                    },
                    Engine = mercedesEngine,
                    Tire = "Bridgestone",
                    Principal = "Martin Whitmarsh",
                    ConstructorsChampionships = 8,
                    DriversChampionships = 12,
                    Races = 678,
                    Victories = 168,
                    Poles = 146,
                    FastestLaps = 140
                },
                new Team
                {
                    Id = Team.Mercedes,
                    Name = "Mercedes GP Petronas F1 Team",
                    Constructor = "Mercedes",
                    Chassis = new Chassis
                    {
                        Name = "MGP W01"
                    },
                    Engine = mercedesEngine,
                    Tire = "Bridgestone",
                    Principal = "Ross Brawn",
                    ConstructorsChampionships = 0,
                    DriversChampionships = 2,
                    Races = 24,
                    Victories = 9,
                    Poles = 8,
                    FastestLaps = 9
                },
                new Team
                {
                    Id = Team.RedBull,
                    Name = "Red Bull Racing",
                    Constructor = "Red Bull",
                    Chassis = new Chassis
                    {
                        Name = "RB6"
                    },
                    Engine = renaultEngine,
                    Tire = "Bridgestone",
                    Principal = "Christian Horner",
                    ConstructorsChampionships = 0,
                    DriversChampionships = 0,
                    Races = 101,
                    Victories = 12,
                    Poles = 16,
                    FastestLaps = 11
                },
                new Team
                {
                    Id = Team.Ferrari,
                    Name = "Scuderia Ferrari Marlboro",
                    Constructor = "Ferrari",
                    Chassis = new Chassis
                    {
                        Name = "F10"
                    },
                    Engine = ferrariEngine,
                    Tire = "Bridgestone",
                    Principal = "Stefano Domenicali",
                    ConstructorsChampionships = 16,
                    DriversChampionships = 15,
                    Races = 805,
                    Victories = 212,
                    Poles = 203,
                    FastestLaps = 221
                },
                new Team
                {
                    Id = Team.Williams,
                    Name = "AT&T Williams",
                    Constructor = "Williams",
                    Chassis = new Chassis
                    {
                        Name = "FW32"
                    },
                    Engine = cosworthEngine,
                    Tire = "Bridgestone",
                    Principal = "Frank Williams/Patrick Head",
                    ConstructorsChampionships = 9,
                    DriversChampionships = 7,
                    Races = 532,
                    Victories = 113,
                    Poles = 125,
                    FastestLaps = 130
                },
                new Team
                {
                    Id = Team.Renault,
                    Name = "Renault F1 Team",
                    Constructor = "Renault",
                    Chassis = new Chassis
                    {
                        Name = "R30"
                    },
                    Engine = renaultEngine,
                    Tire = "Bridgestone",
                    Principal = "Eric Boullier",
                    ConstructorsChampionships = 2,
                    DriversChampionships = 2,
                    Races = 278,
                    Victories = 35,
                    Poles = 51,
                    FastestLaps = 31
                },
                new Team
                {
                    Id = Team.ForceIndia,
                    Name = "Force India F1 Team",
                    Constructor = "Force India",
                    Chassis = new Chassis
                    {
                        Name = "VJM03"
                    },
                    Engine = mercedesEngine,
                    Tire = "Bridgestone",
                    Principal = "Vijay Mallya",
                    ConstructorsChampionships = 0,
                    DriversChampionships = 0,
                    Races = 47,
                    Victories = 0,
                    Poles = 1,
                    FastestLaps = 1
                },
                new Team
                {
                    Id = Team.ToroRosso,
                    Name = "Scuderia Toro Rosso",
                    Constructor = "Toro Rosso",
                    Chassis = new Chassis
                    {
                        Name = "STR5"
                    },
                    Engine = ferrariEngine,
                    Tire = "Bridgestone",
                    Principal = "Franz Tost",
                    ConstructorsChampionships = 0,
                    DriversChampionships = 0,
                    Races = 82,
                    Victories = 1,
                    Poles = 1,
                    FastestLaps = 0
                },
                new Team
                {
                    Id = Team.Lotus,
                    Name = "Lotus Racing",
                    Constructor = "Lotus",
                    Chassis = new Chassis
                    {
                        Name = "T127"
                    },
                    Engine = cosworthEngine,
                    Tire = "Bridgestone",
                    Principal = "Tony Fernandes",
                    ConstructorsChampionships = 7,
                    DriversChampionships = 6,
                    Races = 503,
                    Victories = 73,
                    Poles = 102,
                    FastestLaps = 65
                },
                new Team
                {
                    Id = Team.Hispania,
                    Name = "Hispania Racing F1 Team (HRT)",
                    Constructor = "HRT",
                    Chassis = new Chassis
                    {
                        Name = "F110"
                    },
                    Engine = cosworthEngine,
                    Tire = "Bridgestone",
                    Principal = "Colin Kolles",
                    ConstructorsChampionships = 0,
                    DriversChampionships = 0,
                    Races = 12,
                    Victories = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new Team
                {
                    Id = Team.Sauber,
                    Name = "BMW Sauber F1 Team",
                    Constructor = "Sauber",
                    Chassis = new Chassis
                    {
                        Name = "C29"
                    },
                    Engine = ferrariEngine,
                    Tire = "Bridgestone",
                    Principal = "Peter Sauber",
                    ConstructorsChampionships = 0,
                    DriversChampionships = 0,
                    Races = 288,
                    Victories = 1,
                    Poles = 1,
                    FastestLaps = 2
                },
                new Team
                {
                    Id = Team.Vickers,
                    Name = "Vickers Racing",
                    Constructor = "Vickers",
                    Chassis = new Chassis
                    {
                        Name = "VR-01"
                    },
                    Engine = cosworthEngine,
                    Tire = "Bridgestone",
                    Principal = "John Booth",
                    ConstructorsChampionships = 0,
                    DriversChampionships = 0,
                    Races = 12,
                    Victories = 0,
                    Poles = 0,
                    FastestLaps = 0
                }
            })
            {
                context.Teams.Add(team);
            }

            foreach (var driver in new List<Driver>
            {
                new Driver
                {
                    Name = "Jenson Button",
                    TeamId = Team.McLaren,
                    CarNumber = 1,
                    Championships = 1,
                    Races = 184,
                    Wins = 9,
                    Podiums = 29,
                    Poles = 7,
                    FastestLaps = 3
                },
                new Driver
                {
                    Name = "Lewis Hamilton",
                    TeamId = Team.McLaren,
                    CarNumber = 2,
                    Championships = 1,
                    Races = 64,
                    Wins = 13,
                    Podiums = 33,
                    Poles = 18,
                    FastestLaps = 5
                },
                new TestDriver
                {
                    Name = "Gary Paffett",
                    TeamId = Team.McLaren,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new Driver
                {
                    Name = "Michael Schumacher",
                    TeamId = Team.Mercedes,
                    CarNumber = 3,
                    Championships = 7,
                    Races = 262,
                    Wins = 91,
                    Podiums = 154,
                    Poles = 68,
                    FastestLaps = 76
                },
                new Driver
                {
                    Name = "Nico Rosberg",
                    TeamId = Team.Mercedes,
                    CarNumber = 4,
                    Championships = 0,
                    Races = 82,
                    Wins = 0,
                    Podiums = 5,
                    Poles = 0,
                    FastestLaps = 2
                },
                new TestDriver
                {
                    Name = "Nick Heidfeld",
                    TeamId = Team.Mercedes,
                    CarNumber = null,
                    Championships = 0,
                    Races = 169,
                    Wins = 0,
                    Podiums = 12,
                    Poles = 1,
                    FastestLaps = 2
                },
                new Driver
                {
                    Name = "Sebastian Vettel",
                    TeamId = Team.RedBull,
                    CarNumber = 5,
                    Championships = 0,
                    Races = 55,
                    Wins = 7,
                    Podiums = 15,
                    Poles = 12,
                    FastestLaps = 6
                },
                new Driver
                {
                    Name = "Mark Webber",
                    TeamId = Team.RedBull,
                    CarNumber = 6,
                    Championships = 0,
                    Races = 152,
                    Wins = 6,
                    Podiums = 16,
                    Poles = 5,
                    FastestLaps = 5
                },
                new TestDriver
                {
                    Name = "Brendon Hartley",
                    TeamId = Team.RedBull,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "Daniel Ricciardo",
                    TeamId = Team.RedBull,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "David Coulthard",
                    TeamId = Team.RedBull,
                    CarNumber = null,
                    Championships = 0,
                    Races = 247,
                    Wins = 13,
                    Podiums = 62,
                    Poles = 12,
                    FastestLaps = 18
                },
                new Driver
                {
                    Name = "Felipe Massa",
                    TeamId = Team.Ferrari,
                    CarNumber = 7,
                    Championships = 0,
                    Races = 128,
                    Wins = 11,
                    Podiums = 31,
                    Poles = 15,
                    FastestLaps = 12
                },
                new Driver
                {
                    Name = "Fernando Alonso",
                    TeamId = Team.Ferrari,
                    CarNumber = 8,
                    Championships = 2,
                    Races = 152,
                    Wins = 23,
                    Podiums = 58,
                    Poles = 18,
                    FastestLaps = 15
                },
                new TestDriver
                {
                    Name = "Giancarlo Fisichella",
                    TeamId = Team.Ferrari,
                    CarNumber = null,
                    Championships = 0,
                    Races = 231,
                    Wins = 3,
                    Podiums = 19,
                    Poles = 4,
                    FastestLaps = 2
                },
                new TestDriver
                {
                    Name = "Luca Badoer",
                    TeamId = Team.Ferrari,
                    CarNumber = null,
                    Championships = 0,
                    Races = 58,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "Marc Gené",
                    TeamId = Team.Ferrari,
                    CarNumber = null,
                    Championships = 0,
                    Races = 36,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new Driver
                {
                    Name = "Rubens Barrichello",
                    TeamId = Team.Williams,
                    CarNumber = 9,
                    Championships = 0,
                    Races = 300,
                    Wins = 11,
                    Podiums = 68,
                    Poles = 14,
                    FastestLaps = 17
                },
                new Driver
                {
                    Name = "Nico Hülkenberg",
                    TeamId = Team.Williams,
                    CarNumber = 10,
                    Championships = 0,
                    Races = 12,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "Valtteri Bottas",
                    TeamId = Team.Williams,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new Driver
                {
                    Name = "Robert Kubica",
                    TeamId = Team.Renault,
                    CarNumber = 11,
                    Championships = 0,
                    Races = 69,
                    Wins = 1,
                    Podiums = 11,
                    Poles = 1,
                    FastestLaps = 1
                },
                new Driver
                {
                    Name = "Vitaly Petrov",
                    TeamId = Team.Renault,
                    CarNumber = 12,
                    Championships = 0,
                    Races = 12,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 1
                },
                new TestDriver
                {
                    Name = "Ho-Pin Tung",
                    TeamId = Team.Renault,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "Jérôme d'Ambrosio",
                    TeamId = Team.Renault,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "Jan Charouz",
                    TeamId = Team.Renault,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new Driver
                {
                    Name = "Adrian Sutil",
                    TeamId = Team.ForceIndia,
                    CarNumber = 14,
                    Championships = 0,
                    Races = 64,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 1
                },
                new Driver
                {
                    Name = "Vitantonio Liuzzi",
                    TeamId = Team.ForceIndia,
                    CarNumber = 15,
                    Championships = 0,
                    Races = 56,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "Paul di Resta",
                    TeamId = Team.ForceIndia,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new Driver
                {
                    Name = "Sébastien Buemi",
                    TeamId = Team.ToroRosso,
                    CarNumber = 16,
                    Championships = 0,
                    Races = 29,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new Driver
                {
                    Name = "Jaime Alguersuari",
                    TeamId = Team.ToroRosso,
                    CarNumber = 17,
                    Championships = 0,
                    Races = 20,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "Brendon Hartley",
                    TeamId = Team.ToroRosso,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "Daniel Ricciardo",
                    TeamId = Team.ToroRosso,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new Driver
                {
                    Name = "Jarno Trulli",
                    TeamId = Team.Lotus,
                    CarNumber = 18,
                    Championships = 0,
                    Races = 231,
                    Wins = 1,
                    Podiums = 11,
                    Poles = 4,
                    FastestLaps = 1
                },
                new Driver
                {
                    Name = "Heikki Kovalainen",
                    TeamId = Team.Lotus,
                    CarNumber = 19,
                    Championships = 0,
                    Races = 64,
                    Wins = 1,
                    Podiums = 4,
                    Poles = 1,
                    FastestLaps = 2
                },
                new TestDriver
                {
                    Name = "Fairuz Fauzy",
                    TeamId = Team.Lotus,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new Driver
                {
                    Name = "Karun Chandhok",
                    TeamId = Team.Hispania,
                    CarNumber = 20,
                    Championships = 0,
                    Races = 10,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new Driver
                {
                    Name = "Bruno Senna",
                    TeamId = Team.Hispania,
                    CarNumber = 21,
                    Championships = 0,
                    Races = 11,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "Christian Klien",
                    TeamId = Team.Hispania,
                    CarNumber = null,
                    Championships = 0,
                    Races = 48,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "Sakon Yamamoto",
                    TeamId = Team.Hispania,
                    CarNumber = null,
                    Championships = 0,
                    Races = 17,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new Driver
                {
                    Name = "Timo Glock",
                    TeamId = Team.Vickers,
                    CarNumber = 24,
                    Championships = 0,
                    Races = 48,
                    Wins = 0,
                    Podiums = 3,
                    Poles = 0,
                    FastestLaps = 1
                },
                new Driver
                {
                    Name = "Lucas di Grassi",
                    TeamId = Team.Vickers,
                    CarNumber = 25,
                    Championships = 0,
                    Races = 12,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "Andy Soucek",
                    TeamId = Team.Vickers,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                },
                new TestDriver
                {
                    Name = "Luiz Razia",
                    TeamId = Team.Vickers,
                    CarNumber = null,
                    Championships = 0,
                    Races = 0,
                    Wins = 0,
                    Podiums = 0,
                    Poles = 0,
                    FastestLaps = 0
                }
            })
            {
                context.Drivers.Add(driver);
            }

            var shell = new Sponsor
            {
                Name = "Shell"
            };
            var vodafone = new TitleSponsor
            {
                Name = "Vodafone",
                Details = new SponsorDetails
                {
                    Days = 10,
                    Space = 50m
                }
            };
            var bridgestone = new Sponsor
            {
                Name = "Bridgestone"
            };
            var fia = new Sponsor
            {
                Name = "FIA"
            };

            foreach (var sponsor in new List<Sponsor>
            {
                shell,
                vodafone,
                bridgestone,
                fia
            })
            {
                context.Sponsors.Add(sponsor);
            }

            var teams = context.Teams.Local;
            foreach (var team in teams)
            {
                team.Chassis.TeamId = team.Id;

                if (team.Id != Team.Hispania)
                {
                    team.Sponsors.Add(bridgestone);
                    team.Sponsors.Add(fia);
                }
            }

            teams.Single(t => t.Id == Team.McLaren).Sponsors.Add(vodafone);
            teams.Single(t => t.Id == Team.Ferrari).Sponsors.Add(shell);
        }
    }
}
