// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

#nullable disable

public class GearsOfWarContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<Gear> Gears { get; set; }
    public DbSet<Officer> Officers { get; set; }
    public DbSet<Squad> Squads { get; set; }
    public DbSet<CogTag> Tags { get; set; }
    public DbSet<Weapon> Weapons { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Mission> Missions { get; set; }
    public DbSet<SquadMission> SquadMissions { get; set; }
    public DbSet<Faction> Factions { get; set; }
    public DbSet<LocustLeader> LocustLeaders { get; set; }
    public DbSet<LocustHighCommand> LocustHighCommands { get; set; }

    public static async Task SeedAsync(GearsOfWarContext context)
    {
        var squads = GearsOfWarData.CreateSquads();
        var missions = GearsOfWarData.CreateMissions();
        var squadMissions = GearsOfWarData.CreateSquadMissions();
        var cities = GearsOfWarData.CreateCities();
        var weapons = GearsOfWarData.CreateWeapons();
        var tags = GearsOfWarData.CreateTags();
        var gears = GearsOfWarData.CreateGears();
        var locustLeaders = GearsOfWarData.CreateLocustLeaders();
        var factions = GearsOfWarData.CreateFactions();
        var locustHighCommands = GearsOfWarData.CreateHighCommands();

        GearsOfWarData.WireUp(
            squads, missions, squadMissions, cities, weapons, tags, gears, locustLeaders, factions, locustHighCommands);

        context.Squads.AddRange(squads);
        context.Missions.AddRange(missions);
        context.SquadMissions.AddRange(squadMissions);
        context.Cities.AddRange(cities);
        context.Weapons.AddRange(weapons);
        context.Tags.AddRange(tags);
        context.Gears.AddRange(gears);
        context.LocustLeaders.AddRange(locustLeaders);
        context.Factions.AddRange(factions);
        context.LocustHighCommands.AddRange(locustHighCommands);
        await context.SaveChangesAsync();

        GearsOfWarData.WireUp2(locustLeaders, factions);

        await context.SaveChangesAsync();
    }
}
