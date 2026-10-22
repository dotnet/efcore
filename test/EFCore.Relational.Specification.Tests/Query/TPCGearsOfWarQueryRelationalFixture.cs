// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public abstract class TPCGearsOfWarQueryRelationalFixture : GearsOfWarQueryFixtureBase, ITestSqlLoggerFactory
{
    protected override string StoreName
        => "TPCGearsOfWarQueryTest";

    public new RelationalTestStore TestStore
        => (RelationalTestStore)base.TestStore;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override bool ShouldLogCategory(string logCategory)
        => logCategory == DbLoggerCategory.Query.Name;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            w => w.Log(RelationalEventId.ForeignKeyTpcPrincipalWarning));

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Gear>().UseTpcMappingStrategy();
        modelBuilder.Entity<Faction>().UseTpcMappingStrategy();
        modelBuilder.Entity<LocustLeader>().UseTpcMappingStrategy();

        modelBuilder.Entity<LocustHorde>().ToTable("LocustHordes");

        modelBuilder.Entity<LocustCommander>().ToTable("LocustCommanders");
    }
}
