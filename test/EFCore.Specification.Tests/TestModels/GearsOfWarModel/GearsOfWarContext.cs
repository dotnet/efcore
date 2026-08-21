// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

public class GearsOfWarContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<Gear> Gears { get; set; } = null!;
    public DbSet<Officer> Officers { get; set; } = null!;
    public DbSet<Squad> Squads { get; set; } = null!;
    public DbSet<CogTag> Tags { get; set; } = null!;
    public DbSet<Weapon> Weapons { get; set; } = null!;
    public DbSet<City> Cities { get; set; } = null!;
    public DbSet<Mission> Missions { get; set; } = null!;
    public DbSet<SquadMission> SquadMissions { get; set; } = null!;
    public DbSet<Faction> Factions { get; set; } = null!;
    public DbSet<LocustLeader> LocustLeaders { get; set; } = null!;
    public DbSet<LocustHighCommand> LocustHighCommands { get; set; } = null!;

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
