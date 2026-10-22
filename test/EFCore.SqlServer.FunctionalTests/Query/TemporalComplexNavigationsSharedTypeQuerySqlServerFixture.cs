// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class TemporalComplexNavigationsSharedTypeQuerySqlServerFixture : ComplexNavigationsSharedTypeQuerySqlServerFixture
{
    protected override string StoreName
        => "TemporalComplexNavigationsSharedType";

    public DateTime ChangesDate { get; private set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Level1>().ToTable(
            nameof(Level1), tb => tb.IsTemporal(
                ttb =>
                {
                    ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                    ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                }));
    }

    protected override void Configure(OwnedNavigationBuilder<Level1, Level2> l2)
    {
        base.Configure(l2);

        l2.ToTable(
            nameof(Level1), tb => tb.IsTemporal(
                ttb =>
                {
                    ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                    ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                }));
    }

    protected override void Configure(OwnedNavigationBuilder<Level2, Level3> l3)
    {
        base.Configure(l3);

        l3.ToTable(
            nameof(Level1), tb => tb.IsTemporal(
                ttb =>
                {
                    ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                    ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                }));
    }

    protected override void Configure(OwnedNavigationBuilder<Level3, Level4> l4)
    {
        base.Configure(l4);

        l4.ToTable(
            nameof(Level1), tb => tb.IsTemporal(
                ttb =>
                {
                    ttb.HasPeriodStart("PeriodStart").HasColumnName("PeriodStart");
                    ttb.HasPeriodEnd("PeriodEnd").HasColumnName("PeriodEnd");
                }));
    }

    protected override async Task SeedAsync(ComplexNavigationsContext context)
    {
        await base.SeedAsync(context);

        ChangesDate = new DateTime(2010, 1, 1);

        // clean up intermittent history since in the Seed method we do fixup in multiple stages
        var tableName = nameof(Level1);

        await context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = OFF)");
        await context.Database.ExecuteSqlRawAsync($"DELETE FROM [{tableName + "History"}]");
        await context.Database.ExecuteSqlRawAsync(
            $"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[{tableName + "History"}]))");

        foreach (var entityOne in context.ChangeTracker.Entries().Where(e => e.Entity is Level1).Select(e => e.Entity))
        {
            ((Level1)entityOne).Name += "Modified";
        }

        foreach (var entityOne in context.ChangeTracker.Entries().Where(e => e.Entity is Level2).Select(e => e.Entity))
        {
            ((Level2)entityOne).Name += "Modified";
        }

        foreach (var entityOne in context.ChangeTracker.Entries().Where(e => e.Entity is Level3).Select(e => e.Entity))
        {
            ((Level3)entityOne).Name += "Modified";
        }

        foreach (var entityOne in context.ChangeTracker.Entries().Where(e => e.Entity is Level4).Select(e => e.Entity))
        {
            ((Level4)entityOne).Name += "Modified";
        }

        context.SaveChanges();

        await context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = OFF)");
        await context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{tableName}] DROP PERIOD FOR SYSTEM_TIME");

        await context.Database.ExecuteSqlRawAsync($"UPDATE [{tableName + "History"}] SET PeriodStart = '2000-01-01T01:00:00.0000000Z'");
        await context.Database.ExecuteSqlRawAsync($"UPDATE [{tableName + "History"}] SET PeriodEnd = '2020-07-01T07:00:00.0000000Z'");

        await context.Database.ExecuteSqlRawAsync($"ALTER TABLE [{tableName}] ADD PERIOD FOR SYSTEM_TIME ([PeriodStart], [PeriodEnd])");
        await context.Database.ExecuteSqlRawAsync(
            $"ALTER TABLE [{tableName}] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [dbo].[{tableName + "History"}]))");
    }
}
