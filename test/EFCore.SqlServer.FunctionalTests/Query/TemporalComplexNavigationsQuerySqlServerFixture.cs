// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class TemporalComplexNavigationsQuerySqlServerFixture : ComplexNavigationsQuerySqlServerFixture
    {
        protected override string StoreName { get; } = "TemporalComplexNavigations";

        public DateTime ChangesDate { get; private set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Level1>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<Level2>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<Level3>().ToTable(tb => tb.IsTemporal());
            modelBuilder.Entity<Level4>().ToTable(tb => tb.IsTemporal());
        }

        protected override void Seed(ComplexNavigationsContext context)
        {
            base.Seed(context);

            ChangesDate = new DateTime(2010, 1, 1);

            var historyTableInfos = new List<(string table, string historyTable)>()
            {
                ("LevelOne", "Level1History"),
                ("LevelTwo", "Level2History"),
                ("LevelThree", "Level3History"),
                ("LevelFour", "Level4History"),
            };

            // clean up intermittent history since in the Seed method we do fixup in multiple stages 
            foreach (var historyTableInfo in historyTableInfos)
            {
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{historyTableInfo.table}] SET (SYSTEM_VERSIONING = OFF)");
                context.Database.ExecuteSqlRaw($"DELETE FROM [{historyTableInfo.historyTable}]");
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{historyTableInfo.table}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[{historyTableInfo.historyTable}]))");
            }

            foreach (var entityOne in context.ChangeTracker.Entries().Where(e => e.Entity is Level1).Select(e => e.Entity))
            {
                ((Level1)entityOne).Name = ((Level1)entityOne).Name + "Modified";
            }

            foreach (var entityOne in context.ChangeTracker.Entries().Where(e => e.Entity is Level2).Select(e => e.Entity))
            {
                ((Level2)entityOne).Name = ((Level2)entityOne).Name + "Modified";
            }

            foreach (var entityOne in context.ChangeTracker.Entries().Where(e => e.Entity is Level3).Select(e => e.Entity))
            {
                ((Level3)entityOne).Name = ((Level3)entityOne).Name + "Modified";
            }

            foreach (var entityOne in context.ChangeTracker.Entries().Where(e => e.Entity is Level4).Select(e => e.Entity))
            {
                ((Level4)entityOne).Name = ((Level4)entityOne).Name + "Modified";
            }

            context.SaveChanges();

            foreach (var historyTableInfo in historyTableInfos)
            {
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{historyTableInfo.table}] SET (SYSTEM_VERSIONING = OFF)");
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{historyTableInfo.table}] DROP PERIOD FOR SYSTEM_TIME");

                context.Database.ExecuteSqlRaw($"UPDATE [{historyTableInfo.historyTable}] SET PeriodStart = '2000-01-01T01:00:00.0000000Z'");
                context.Database.ExecuteSqlRaw($"UPDATE [{historyTableInfo.historyTable}] SET PeriodEnd = '2020-07-01T07:00:00.0000000Z'");

                context.Database.ExecuteSqlRaw($"ALTER TABLE [{historyTableInfo.table}] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])");
                context.Database.ExecuteSqlRaw($"ALTER TABLE [{historyTableInfo.table}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[{historyTableInfo.historyTable}]))");
            }
        }
    }
}
