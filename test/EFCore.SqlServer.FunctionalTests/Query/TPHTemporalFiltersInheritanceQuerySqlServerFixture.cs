// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class TPHTemporalFiltersInheritanceQuerySqlServerFixture : TPHFiltersInheritanceQuerySqlServerFixture
{
    protected override string StoreName
        => "TemporalFiltersInheritanceQueryTest";

    public DateTime ChangesDate { get; private set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Animal>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<Plant>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<Country>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<Drink>().ToTable(tb => tb.IsTemporal());
    }

    protected override async Task SeedAsync(InheritanceContext context)
    {
        await base.SeedAsync(context);

        ChangesDate = new DateTime(2010, 1, 1);

        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is Animal).Select(e => e.Entity));
        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is Plant).Select(e => e.Entity));
        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is Country).Select(e => e.Entity));
        context.RemoveRange(context.ChangeTracker.Entries().Where(e => e.Entity is Drink).Select(e => e.Entity));
        await context.SaveChangesAsync();

        var tableNames = new List<string>
        {
            "Animals",
            "Plants",
            "Countries",
            "Drinks"
        };

        foreach (var tableName in tableNames)
        {
            await context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = OFF)");
            await context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{tableName}] DROP PERIOD FOR SYSTEM_TIME");

            await context.Database.ExecuteSqlRawAsync($"UPDATE [{tableName + "History"}] SET PeriodStart = '2000-01-01T01:00:00.0000000Z'");
            await context.Database.ExecuteSqlRawAsync($"UPDATE [{tableName + "History"}] SET PeriodEnd = '2020-07-01T07:00:00.0000000Z'");

            await context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{tableName}] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])");
            await context.Database.ExecuteSqlRawAsync(
                $"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[{tableName + "History"}]))");
        }
    }
}
