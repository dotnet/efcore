// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

public abstract class TPCGearsOfWarQueryRelationalFixture : GearsOfWarQueryFixtureBase
{
    protected override string StoreName { get; } = "TPCGearsOfWarQueryTest";

    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Query.Name;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            w =>
                w.Log(RelationalEventId.ForeignKeyTpcPrincipalWarning));

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Gear>().UseTpcMappingStrategy();
        modelBuilder.Entity<Faction>().UseTpcMappingStrategy();
        modelBuilder.Entity<LocustLeader>().UseTpcMappingStrategy();

        // Work-around for issue#27947
        modelBuilder.Entity<Faction>().ToTable((string)null);

        modelBuilder.Entity<Gear>().ToTable("Gears");
        modelBuilder.Entity<Officer>().ToTable("Officers");

        modelBuilder.Entity<LocustHorde>().ToTable("LocustHordes");

        modelBuilder.Entity<LocustLeader>().ToTable("LocustLeaders");
        modelBuilder.Entity<LocustCommander>().ToTable("LocustCommanders");
    }
}
