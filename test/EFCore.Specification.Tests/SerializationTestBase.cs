// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class SerializationTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : F1FixtureBase, new()
    {
        protected SerializationTestBase(TFixture fixture)
            => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalTheory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        public virtual void Can_round_trip_through_JSON(bool useNewtonsoft, bool ignoreLoops, bool writeIndented)
        {
            using var context = Fixture.CreateContext();

            var teams = context.Teams.Include(e => e.Drivers)
                .Include(e => e.Engine).ThenInclude(e => e.EngineSupplier)
                .ToList();

            Assert.Equal(12, teams.Count);
            Assert.Equal(42, teams.SelectMany(e => e.Drivers).Count());
            Assert.True(teams.All(e => e.Engine != null));
            Assert.True(teams.All(e => e.Engine.EngineSupplier != null));

            var teamsAgain = useNewtonsoft
                ? RoundtripThroughNewtonsoftJson(teams, ignoreLoops, writeIndented)
                : RoundtripThroughBclJson(teams, ignoreLoops, writeIndented);

            var teamsMap = ignoreLoops ? null : new Dictionary<int, Team>();
            var enginesMap = ignoreLoops ? null : new Dictionary<int, Engine>();
            var engineSupplierMap = ignoreLoops ? null : new Dictionary<int, EngineSupplier>();

            foreach (var team in teamsAgain)
            {
                VerifyTeam(context, team, teamsMap);
                VerifyEngine(context, team.Engine, enginesMap);
                VerifyEngineSupplier(context, team.Engine.EngineSupplier, engineSupplierMap);
            }
        }

        private static void VerifyTeam(F1Context context, Team team, IDictionary<int, Team> teamsMap)
        {
            var trackedTeam = context.Teams.Find(team.Id);
            Assert.Equal(trackedTeam.Constructor, team.Constructor);
            Assert.Equal(trackedTeam.Name, team.Name);
            Assert.Equal(trackedTeam.Poles, team.Poles);
            Assert.Equal(trackedTeam.Principal, team.Principal);
            Assert.Equal(trackedTeam.Races, team.Races);
            Assert.Equal(trackedTeam.Tire, team.Tire);
            Assert.Equal(trackedTeam.Victories, team.Victories);
            Assert.Equal(trackedTeam.ConstructorsChampionships, team.ConstructorsChampionships);
            Assert.Equal(trackedTeam.DriversChampionships, team.DriversChampionships);
            Assert.Equal(trackedTeam.FastestLaps, team.FastestLaps);

            if (teamsMap != null)
            {
                if (teamsMap.TryGetValue(team.Id, out var mappedTeam))
                {
                    Assert.Same(team, mappedTeam);
                }

                teamsMap[team.Id] = team;
            }
        }

        private static void VerifyEngine(F1Context context, Engine engine, IDictionary<int, Engine> enginesMap)
        {
            var trackedEngine = context.Engines.Find(engine.Id);
            Assert.Equal(trackedEngine.StorageLocation.Latitude, engine.StorageLocation.Latitude);
            Assert.Equal(trackedEngine.StorageLocation.Longitude, engine.StorageLocation.Longitude);
            Assert.Equal(trackedEngine.Name, engine.Name);

            if (enginesMap != null)
            {
                if (enginesMap.TryGetValue(engine.Id, out var mappedEngine))
                {
                    Assert.Same(engine, mappedEngine);
                }

                enginesMap[engine.Id] = engine;
            }
        }

        private static void VerifyEngineSupplier(
            F1Context context,
            EngineSupplier engineSupplier,
            IDictionary<int, EngineSupplier> engineSupplierMap)
        {
            var trackedEngineSupplier = context.EngineSuppliers.Find(engineSupplier.Id);
            Assert.Equal(trackedEngineSupplier.Name, engineSupplier.Name);

            if (engineSupplierMap != null)
            {
                if (engineSupplierMap.TryGetValue(engineSupplier.Id, out var mappedEngineSupplier))
                {
                    Assert.Same(engineSupplier, mappedEngineSupplier);
                }

                engineSupplierMap[engineSupplier.Id] = engineSupplier;
            }
        }

        private static T RoundtripThroughBclJson<T>(T collection, bool ignoreLoops, bool writeIndented, int maxDepth = 64)
        {
            Assert.False(ignoreLoops, "BCL doesn't support ignoring loops.");

#if NET5_0
            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                WriteIndented = writeIndented,
                MaxDepth = maxDepth
            };

            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(collection, options), options);
#else
            return collection;
#endif
        }

        private static T RoundtripThroughNewtonsoftJson<T>(T collection, bool ignoreLoops, bool writeIndented)
        {
            var options = new Newtonsoft.Json.JsonSerializerSettings
            {
                PreserveReferencesHandling = ignoreLoops
                    ? Newtonsoft.Json.PreserveReferencesHandling.None
                    : Newtonsoft.Json.PreserveReferencesHandling.All,
                ReferenceLoopHandling = ignoreLoops
                    ? Newtonsoft.Json.ReferenceLoopHandling.Ignore
                    : Newtonsoft.Json.ReferenceLoopHandling.Error,
                EqualityComparer = LegacyReferenceEqualityComparer.Instance,
                Formatting = writeIndented
                    ? Newtonsoft.Json.Formatting.Indented
                    : Newtonsoft.Json.Formatting.None
            };

            var serializeObject = Newtonsoft.Json.JsonConvert.SerializeObject(collection, options);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(serializeObject);
        }
    }
}
