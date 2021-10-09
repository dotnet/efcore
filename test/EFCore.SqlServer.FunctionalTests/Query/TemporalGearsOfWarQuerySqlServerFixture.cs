// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class TemporalGearsOfWarQuerySqlServerFixture : GearsOfWarQuerySqlServerFixture
    {
        protected override string StoreName { get; } = "TemporalGearsOfWarQueryTest";

        public DateTime ChangesDate { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<City>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<CogTag>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<Faction>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<Gear>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<LocustLeader>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<LocustHighCommand>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<Mission>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<Squad>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<SquadMission>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<Weapon>().ToTable(tb => tb.IsTemporal());

            base.OnModelCreating(modelBuilder, context);
        }

        protected override void Seed(GearsOfWarContext context)
        {
            base.Seed(context);

            ChangesDate = new DateTime(2010, 1, 1);

            //// clean up intermittent history - we do the data fixup in 2 steps (due to cycle)
            //// so we want to remove the temporary states, so that further manipulation is easier
            context.Database.ExecuteSqlRaw("ALTER TABLE [LocustLeaders] SET (SYSTEM_VERSIONING = OFF)");
            context.Database.ExecuteSqlRaw("DELETE FROM [LocustLeadersHistory]");
            context.Database.ExecuteSqlRaw("ALTER TABLE [LocustLeaders] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[LocustLeadersHistory]))");

            context.Database.ExecuteSqlRaw("ALTER TABLE [Missions] SET (SYSTEM_VERSIONING = OFF)");
            context.Database.ExecuteSqlRaw("DELETE FROM [MissionsHistory]");
            context.Database.ExecuteSqlRaw("ALTER TABLE [Missions] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[MissionsHistory]))");

            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is City).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is CogTag).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is Gear).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is LocustHighCommand).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is Mission).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is Squad).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is SquadMission).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is Weapon).Select(e => e.Entity));
            context.SaveChanges();

            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is Faction).Select(e => e.Entity));
            context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is LocustLeader).Select(e => e.Entity));
            context.SaveChanges();

            // clean up Faction history
            context.Database.ExecuteSqlRaw("ALTER TABLE [Factions] SET (SYSTEM_VERSIONING = OFF)");
            context.Database.ExecuteSqlRaw("DELETE FROM [FactionsHistory] WHERE CommanderName IS NULL");
            context.Database.ExecuteSqlRaw("ALTER TABLE [Factions] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[FactionsHistory]))");

            var tableNames = new List<string>
            {
                "Cities",
                "Tags",
                "Gears",
                "LocustHighCommands",
                "Missions",
                "Squads",
                "SquadMissions",
                "Weapons",

                "LocustLeaders",
                "Factions",
            };

            foreach (var tableName in tableNames)
            {
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = OFF)");
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] DROP PERIOD FOR SYSTEM_TIME");

                context.Database.ExecuteSqlRaw($"UPDATE [{tableName + "History"}] SET PeriodStart = '2000-01-01T01:00:00.0000000Z'");
                context.Database.ExecuteSqlRaw($"UPDATE [{tableName + "History"}] SET PeriodEnd = '2020-07-01T07:00:00.0000000Z'");

                context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])");
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[{tableName + "History"}]))");
            }
        }
    }
}
