// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

public class GearsOfWarODataContext(DbContextOptions options) : PoolableDbContext(options)
{
    public DbSet<Gear> Gears { get; set; }
    public DbSet<Squad> Squads { get; set; }
    public DbSet<CogTag> Tags { get; set; }
    public DbSet<Weapon> Weapons { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Mission> Missions { get; set; }
    public DbSet<SquadMission> SquadMissions { get; set; }
    public DbSet<Faction> Factions { get; set; }
    public DbSet<LocustLeader> LocustLeaders { get; set; }
    public DbSet<LocustHighCommand> LocustHighCommands { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
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

        modelBuilder.Entity<City>().Property(g => g.Location).HasColumnType("varchar(100)");
    }
}
