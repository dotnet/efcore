// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Net.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarODataQueryTestFixture : GearsOfWarQuerySqlServerFixture, IODataQueryTestFixture
    {
        private IHost _selfHostServer = null;

        protected override string StoreName { get; } = "ODataGearsOfWarQueryTest";

        public GearsOfWarODataQueryTestFixture()
        {
            var controllers = new Type[]
            {
                typeof(GearsController),
                typeof(SquadsController),
                typeof(TagsController),
                typeof(WeaponsController),
                typeof(CitiesController),
                typeof(MissionsController),
                typeof(SquadMissionsController),
                typeof(FactionsController),
                typeof(LocustLeadersController),
                typeof(LocustHighCommandsController),
            };

            (BaseAddress, ClientFactory, _selfHostServer)
                = ODataQueryTestFixtureInitializer.Initialize<GearsOfWarODataContext>(StoreName, controllers, GetEdmModel());
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

        public string BaseAddress { get; private set; }

        public IHttpClientFactory ClientFactory { get; private set; }

        public override void Dispose()
        {
            if (_selfHostServer != null)
            {
                //issue: dotnet/runtime #35990
                _selfHostServer.StopAsync();
                System.Threading.Thread.Sleep(5000);
                _selfHostServer.Dispose();
                //_selfHostServer.WaitForShutdown();

                _selfHostServer = null;
            }
        }
    }
}
