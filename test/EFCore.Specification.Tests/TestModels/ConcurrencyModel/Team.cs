// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;

namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

#nullable disable

public class Team
{
    public class TeamProxy(
        ILazyLoader loader,
        int id,
        string name,
        string constructor,
        string tire,
        string principal,
        int constructorsChampionships,
        int driversChampionships,
        int races,
        int victories,
        int poles,
        int fastestLaps,
        int? gearboxId) : Team(
            loader, id, name, constructor, tire, principal, constructorsChampionships, driversChampionships, races, victories, poles,
            fastestLaps, gearboxId), IF1Proxy
    {
        public bool CreatedCalled { get; set; }
        public bool InitializingCalled { get; set; }
        public bool InitializedCalled { get; set; }
    }

    private readonly ILazyLoader _loader;
    private readonly ObservableCollection<Driver> _drivers = new ObservableCollectionListSource<Driver>();
    private readonly ObservableCollection<Sponsor> _sponsors = [];
    private Engine _engine;
    private Chassis _chassis;
    private Gearbox _gearbox;

    public Team()
    {
    }

    private Team(
        ILazyLoader loader,
        int id,
        string name,
        string constructor,
        string tire,
        string principal,
        int constructorsChampionships,
        int driversChampionships,
        int races,
        int victories,
        int poles,
        int fastestLaps,
        int? gearboxId)
    {
        _loader = loader;
        Id = id;
        Name = name;
        Constructor = constructor;
        Tire = tire;
        Principal = principal;
        ConstructorsChampionships = constructorsChampionships;
        DriversChampionships = driversChampionships;
        Races = races;
        Victories = victories;
        Poles = poles;
        FastestLaps = fastestLaps;
        GearboxId = gearboxId;

        Assert.IsType<TeamProxy>(this);
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Constructor { get; set; }
    public string Tire { get; set; }
    public string Principal { get; set; }
    public int ConstructorsChampionships { get; set; }
    public int DriversChampionships { get; set; }
    public int Races { get; set; }
    public int Victories { get; set; }
    public int Poles { get; set; }
    public int FastestLaps { get; set; }

    public virtual Engine Engine
    {
        get => _loader.Load(this, ref _engine);
        set => _engine = value;
    }

    public virtual Chassis Chassis
    {
        get => _loader.Load(this, ref _chassis);
        set => _chassis = value;
    }

    public virtual ICollection<Driver> Drivers
    {
        get
        {
            _loader?.Load(this);
            return _drivers;
        }
    }

    public virtual ICollection<Sponsor> Sponsors
        => _sponsors;

    public int? GearboxId { get; set; }

    public virtual Gearbox Gearbox
    {
        get => _loader.Load(this, ref _gearbox);
        set => _gearbox = value;
    }

    public const int McLaren = 1;
    public const int Mercedes = 2;
    public const int RedBull = 3;
    public const int Ferrari = 4;
    public const int Williams = 5;
    public const int Renault = 6;
    public const int ForceIndia = 7;
    public const int ToroRosso = 8;
    public const int Lotus = 9;
    public const int Hispania = 10;
    public const int Sauber = 11;
    public const int Vickers = 12;
}
