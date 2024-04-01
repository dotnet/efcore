// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class GearsOfWarQueryFixtureBase : SharedStoreFixtureBase<GearsOfWarContext>, IQueryFixtureBase
{
    protected override string StoreName
        => "GearsOfWarQueryTest";

    public Func<DbContext> GetContextCreator()
        => () => CreateContext();

    public virtual ISetSource GetExpectedData()
        => GearsOfWarData.Instance;

    public virtual Dictionary<(Type, string), Func<object, object>> GetShadowPropertyMappings()
        => new()
        {
            {
                (typeof(Gear), "AssignedCityName"), e => GetExpectedData().Set<Gear>().AsEnumerable()
                    .SingleOrDefault(g => g.Nickname == ((Gear)e)?.Nickname)?.AssignedCity
                    ?.Name
            },
        };

    public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
    {
        { typeof(City), e => ((City)e)?.Name },
        { typeof(CogTag), e => ((CogTag)e)?.Id },
        { typeof(Faction), e => ((Faction)e)?.Id },
        { typeof(LocustHorde), e => ((LocustHorde)e)?.Id },
        { typeof(Gear), e => (((Gear)e)?.SquadId, ((Gear)e)?.Nickname) },
        { typeof(Officer), e => (((Officer)e)?.SquadId, ((Officer)e)?.Nickname) },
        { typeof(LocustLeader), e => ((LocustLeader)e)?.Name },
        { typeof(LocustCommander), e => ((LocustCommander)e)?.Name },
        { typeof(Mission), e => ((Mission)e)?.Id },
        { typeof(Squad), e => ((Squad)e)?.Id },
        { typeof(SquadMission), e => (((SquadMission)e)?.SquadId, ((SquadMission)e)?.MissionId) },
        { typeof(Weapon), e => ((Weapon)e)?.Id },
        { typeof(LocustHighCommand), e => ((LocustHighCommand)e)?.Id }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
    {
        {
            typeof(City), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (City)e;
                    var aa = (City)a;

                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Location, aa.Location);
                    Assert.Equal(ee.Nation, aa.Nation);
                }
            }
        },
        {
            typeof(CogTag), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (CogTag)e;
                    var aa = (CogTag)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Note, aa.Note);
                    Assert.Equal(ee.GearNickName, aa.GearNickName);
                    Assert.Equal(ee.GearSquadId, aa.GearSquadId);
                }
            }
        },
        {
            typeof(Faction), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Faction)e;
                    var aa = (Faction)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.CapitalName, aa.CapitalName);
                    if (ee is LocustHorde locustHorde)
                    {
                        var actualLocustHorde = (LocustHorde)aa;
                        Assert.Equal(locustHorde.CommanderName, actualLocustHorde.CommanderName);
                        Assert.Equal(locustHorde.Eradicated, actualLocustHorde.Eradicated);
                    }
                }
            }
        },
        {
            typeof(LocustHorde), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (LocustHorde)e;
                    var aa = (LocustHorde)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.CapitalName, aa.CapitalName);
                    Assert.Equal(ee.CommanderName, aa.CommanderName);
                    Assert.Equal(ee.Eradicated, aa.Eradicated);
                }
            }
        },
        {
            typeof(Gear), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Gear)e;
                    var aa = (Gear)a;

                    Assert.Equal(ee.Nickname, aa.Nickname);
                    Assert.Equal(ee.SquadId, aa.SquadId);
                    Assert.Equal(ee.CityOfBirthName, aa.CityOfBirthName);
                    Assert.Equal(ee.FullName, aa.FullName);
                    Assert.Equal(ee.HasSoulPatch, aa.HasSoulPatch);
                    Assert.Equal(ee.LeaderNickname, aa.LeaderNickname);
                    Assert.Equal(ee.LeaderSquadId, aa.LeaderSquadId);
                    Assert.Equal(ee.Rank, aa.Rank);
                }
            }
        },
        {
            typeof(Officer), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Officer)e;
                    var aa = (Officer)a;

                    Assert.Equal(ee.Nickname, aa.Nickname);
                    Assert.Equal(ee.SquadId, aa.SquadId);
                    Assert.Equal(ee.CityOfBirthName, aa.CityOfBirthName);
                    Assert.Equal(ee.FullName, aa.FullName);
                    Assert.Equal(ee.HasSoulPatch, aa.HasSoulPatch);
                    Assert.Equal(ee.LeaderNickname, aa.LeaderNickname);
                    Assert.Equal(ee.LeaderSquadId, aa.LeaderSquadId);
                    Assert.Equal(ee.Rank, aa.Rank);
                }
            }
        },
        {
            typeof(LocustLeader), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (LocustLeader)e;
                    var aa = (LocustLeader)a;

                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ThreatLevel, aa.ThreatLevel);
                    Assert.Equal(ee.ThreatLevelByte, aa.ThreatLevelByte);
                    Assert.Equal(ee.ThreatLevelNullableByte, aa.ThreatLevelNullableByte);

                    if (e is LocustCommander locustCommander)
                    {
                        var actualLocustCommander = (LocustCommander)aa;
                        Assert.Equal(locustCommander.DefeatedByNickname, actualLocustCommander.DefeatedByNickname);
                        Assert.Equal(locustCommander.DefeatedBySquadId, actualLocustCommander.DefeatedBySquadId);
                    }
                }
            }
        },
        {
            typeof(LocustCommander), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (LocustCommander)e;
                    var aa = (LocustCommander)a;

                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.ThreatLevel, aa.ThreatLevel);
                    Assert.Equal(ee.ThreatLevelByte, aa.ThreatLevelByte);
                    Assert.Equal(ee.ThreatLevelNullableByte, aa.ThreatLevelNullableByte);
                    Assert.Equal(ee.DefeatedByNickname, aa.DefeatedByNickname);
                    Assert.Equal(ee.DefeatedBySquadId, aa.DefeatedBySquadId);
                }
            }
        },
        {
            typeof(Mission), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Mission)e;
                    var aa = (Mission)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.CodeName, aa.CodeName);
                    Assert.Equal(ee.Rating, aa.Rating);
                    Assert.Equal(ee.Timeline, aa.Timeline);
                }
            }
        },
        {
            typeof(Squad), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Squad)e;
                    var aa = (Squad)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.Banner == null, aa.Banner == null);
                    if (ee.Banner != null)
                    {
                        Assert.True(ee.Banner.SequenceEqual(aa.Banner));
                    }

                    Assert.Equal(ee.Banner5 == null, aa.Banner5 == null);
                    if (ee.Banner5 != null)
                    {
                        Assert.True(ee.Banner5.SequenceEqual(aa.Banner5));
                    }
                }
            }
        },
        {
            typeof(SquadMission), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (SquadMission)e;
                    var aa = (SquadMission)a;

                    Assert.Equal(ee.SquadId, aa.SquadId);
                    Assert.Equal(ee.MissionId, aa.MissionId);
                }
            }
        },
        {
            typeof(Weapon), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (Weapon)e;
                    var aa = (Weapon)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.IsAutomatic, aa.IsAutomatic);
                    Assert.Equal(ee.Name, aa.Name);
                    Assert.Equal(ee.OwnerFullName, aa.OwnerFullName);
                    Assert.Equal(ee.SynergyWithId, aa.SynergyWithId);
                }
            }
        },
        {
            typeof(LocustHighCommand), (e, a) =>
            {
                Assert.Equal(e == null, a == null);

                if (a != null)
                {
                    var ee = (LocustHighCommand)e;
                    var aa = (LocustHighCommand)a;

                    Assert.Equal(ee.Id, aa.Id);
                    Assert.Equal(ee.IsOperational, aa.IsOperational);
                    Assert.Equal(ee.Name, aa.Name);
                }
            }
        }
    }.ToDictionary(e => e.Key, e => (object)e.Value);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<City>(
            b =>
            {
                b.HasKey(c => c.Name);
            });

        modelBuilder.Entity<Gear>(
            b =>
            {
                b.HasKey(
                    g => new { g.Nickname, g.SquadId });

                b.HasOne(g => g.CityOfBirth).WithMany(c => c.BornGears).HasForeignKey(g => g.CityOfBirthName).IsRequired();
                b.HasOne(g => g.Tag).WithOne(t => t.Gear).HasForeignKey<CogTag>(
                    t => new { t.GearNickName, t.GearSquadId });
                b.HasOne(g => g.AssignedCity).WithMany(c => c.StationedGears).IsRequired(false);
            });

        modelBuilder.Entity<Officer>().HasMany(o => o.Reports).WithOne().HasForeignKey(
            o => new { o.LeaderNickname, o.LeaderSquadId });

        modelBuilder.Entity<Squad>(
            b =>
            {
                b.HasKey(s => s.Id);
                b.Property(s => s.Id).ValueGeneratedNever();
                b.Property(s => s.Banner5).HasMaxLength(5);
                b.HasMany(s => s.Members).WithOne(g => g.Squad).HasForeignKey(g => g.SquadId);
            });

        modelBuilder.Entity<Weapon>(
            b =>
            {
                b.Property(w => w.Id).ValueGeneratedNever();
                b.HasOne(w => w.SynergyWith).WithOne().HasForeignKey<Weapon>(w => w.SynergyWithId);
                b.HasOne(w => w.Owner).WithMany(g => g.Weapons).HasForeignKey(w => w.OwnerFullName).HasPrincipalKey(g => g.FullName);
            });

        modelBuilder.Entity<Mission>().Property(m => m.Id).ValueGeneratedNever();

        modelBuilder.Entity<SquadMission>(
            b =>
            {
                b.HasKey(
                    sm => new { sm.SquadId, sm.MissionId });
                b.HasOne(sm => sm.Mission).WithMany(m => m.ParticipatingSquads).HasForeignKey(sm => sm.MissionId);
                b.HasOne(sm => sm.Squad).WithMany(s => s.Missions).HasForeignKey(sm => sm.SquadId);
            });

        modelBuilder.Entity<Faction>().HasKey(f => f.Id);
        modelBuilder.Entity<Faction>().Property(f => f.Id).ValueGeneratedNever();

        modelBuilder.Entity<LocustHorde>().HasBaseType<Faction>();
        modelBuilder.Entity<LocustHorde>().HasMany(h => h.Leaders).WithOne();

        modelBuilder.Entity<LocustHorde>().HasOne(h => h.Commander).WithOne(c => c.CommandingFaction);

        modelBuilder.Entity<LocustLeader>().HasKey(l => l.Name);
        modelBuilder.Entity<LocustCommander>().HasBaseType<LocustLeader>();
        modelBuilder.Entity<LocustCommander>().HasOne(c => c.DefeatedBy).WithOne().HasForeignKey<LocustCommander>(
            c => new { c.DefeatedByNickname, c.DefeatedBySquadId });

        modelBuilder.Entity<LocustHighCommand>().HasKey(l => l.Id);
        modelBuilder.Entity<LocustHighCommand>().Property(l => l.Id).ValueGeneratedNever();
    }

    protected override Task SeedAsync(GearsOfWarContext context)
        => GearsOfWarContext.SeedAsync(context);

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            w =>
                w.Log(CoreEventId.RowLimitingOperationWithoutOrderByWarning));

    public override GearsOfWarContext CreateContext()
    {
        var context = base.CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        return context;
    }
}
