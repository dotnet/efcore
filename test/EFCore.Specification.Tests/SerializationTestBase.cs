// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class SerializationTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : F1FixtureBase<byte[]>, new()
{
    protected SerializationTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

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

        var teams = context.Teams.ToList();

        Assert.Equal(12, teams.Count);
        Assert.Equal(42, teams.SelectMany(e => e.Drivers).Count());
        Assert.True(teams.All(e => e.Engine != null));
        Assert.True(teams.All(e => e.Engine.EngineSupplier != null));

        var teamsAgain = useNewtonsoft
            ? RoundtripThroughNewtonsoftJson(teams, ignoreLoops, writeIndented)
            : RoundtripThroughBclJson(teams, ignoreLoops, writeIndented);

        var teamsMap = ignoreLoops ? null : new Dictionary<int, Team>();
        var enginesMap = ignoreLoops ? null : new Dictionary<int, Engine>();
        var engineSupplierMap = ignoreLoops ? null : new Dictionary<string, EngineSupplier>();

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
        IDictionary<string, EngineSupplier> engineSupplierMap)
    {
        var trackedEngineSupplier = context.EngineSuppliers.Find(engineSupplier.Name);
        Assert.Equal(trackedEngineSupplier.Name, engineSupplier.Name);

        if (engineSupplierMap != null)
        {
            if (engineSupplierMap.TryGetValue(engineSupplier.Name, out var mappedEngineSupplier))
            {
                Assert.Same(engineSupplier, mappedEngineSupplier);
            }

            engineSupplierMap[engineSupplier.Name] = engineSupplier;
        }
    }

    private static T RoundtripThroughBclJson<T>(T collection, bool ignoreLoops, bool writeIndented, int maxDepth = 64)
    {
        Assert.False(ignoreLoops, "BCL doesn't support ignoring loops.");

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve,
            WriteIndented = writeIndented,
            MaxDepth = maxDepth
        };

        return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(collection, options), options);
    }

    private static T RoundtripThroughNewtonsoftJson<T>(T collection, bool ignoreLoops, bool writeIndented)
    {
        var options = new JsonSerializerSettings
        {
            PreserveReferencesHandling = ignoreLoops
                ? PreserveReferencesHandling.None
                : PreserveReferencesHandling.All,
            ReferenceLoopHandling = ignoreLoops
                ? ReferenceLoopHandling.Ignore
                : ReferenceLoopHandling.Error,
            EqualityComparer = ReferenceEqualityComparer.Instance,
            Formatting = writeIndented
                ? Formatting.Indented
                : Formatting.None
        };

        var serializeObject = JsonConvert.SerializeObject(collection, options);

        return JsonConvert.DeserializeObject<T>(serializeObject);
    }
}
