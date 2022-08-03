// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

public class Driver
{
    public class DriverProxy : Driver, IF1Proxy
    {
        public DriverProxy(
            ILazyLoader loader,
            int id,
            string name,
            int? carNumber,
            int championships,
            int races,
            int wins,
            int podiums,
            int poles,
            int fastestLaps,
            int teamId)
            : base(loader, id, name, carNumber, championships, races, wins, podiums, poles, fastestLaps, teamId)
        {
        }

        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    private readonly ILazyLoader _loader;
    private Team _team;

    public Driver()
    {
    }

    protected Driver(
        ILazyLoader loader,
        int id,
        string name,
        int? carNumber,
        int championships,
        int races,
        int wins,
        int podiums,
        int poles,
        int fastestLaps,
        int teamId)
    {
        _loader = loader;

        Id = id;
        Name = name;
        CarNumber = carNumber;
        Championships = championships;
        Races = races;
        Wins = wins;
        Podiums = podiums;
        Poles = poles;
        FastestLaps = fastestLaps;
        TeamId = teamId;

        Assert.True(this is DriverProxy || this is TestDriver);
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public int? CarNumber { get; set; }
    public int Championships { get; set; }
    public int Races { get; set; }
    public int Wins { get; set; }
    public int Podiums { get; set; }
    public int Poles { get; set; }
    public int FastestLaps { get; set; }

    public virtual Team Team
    {
        get => _loader.Load(this, ref _team);
        set => _team = value;
    }

    public int TeamId { get; set; }
}
