// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace Microsoft.EntityFrameworkCore.Query;

public class GearsOfWarODataQueryTestFixture : GearsOfWarQuerySqlServerFixture, IODataQueryTestFixture
{
    private IHost _selfHostServer;

    protected override string StoreName
        => "ODataGearsOfWarQueryTest";

    public GearsOfWarODataQueryTestFixture()
    {
        (BaseAddress, ClientFactory, _selfHostServer)
            = ODataQueryTestFixtureInitializer.Initialize<GearsOfWarODataContext>(StoreName, GetEdmModel());
    }

    private static IEdmModel GetEdmModel()
    {
        var modelBuilder = new ODataConventionModelBuilder();
        modelBuilder.EntitySet<Gear>("Gears");
        modelBuilder.EntityType<Gear>().HasKey(e => new { e.Nickname, e.SquadId });
        modelBuilder.EntitySet<Squad>("Squads");
        modelBuilder.EntitySet<CogTag>("Tags");
        modelBuilder.EntitySet<Weapon>("Weapons");
        modelBuilder.EntitySet<City>("Cities");
        modelBuilder.EntityType<City>().HasKey(c => c.Name);
        modelBuilder.EntitySet<Mission>("Missions");
        modelBuilder.EntitySet<SquadMission>("SquadMissions");
        modelBuilder.EntityType<SquadMission>().HasKey(e => new { e.SquadId, e.MissionId });
        modelBuilder.EntitySet<Faction>("Factions");
        modelBuilder.EntitySet<LocustLeader>("LocustLeaders");
        modelBuilder.EntityType<LocustLeader>().HasKey(c => c.Name);
        modelBuilder.EntitySet<LocustHighCommand>("LocustHighCommands");

        return modelBuilder.GetEdmModel();
    }

    public string BaseAddress { get; }

    public IHttpClientFactory ClientFactory { get; }

    public override async Task DisposeAsync()
    {
        if (_selfHostServer != null)
        {
            await _selfHostServer.StopAsync();
            _selfHostServer.Dispose();
            _selfHostServer = null;
        }
    }
}
