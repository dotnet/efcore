// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

namespace Microsoft.EntityFrameworkCore;

public abstract class F1RelationalFixture<TRowVersion> : F1FixtureBase<TRowVersion>
{
    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            w => w.Ignore(RelationalEventId.BatchSmallerThanMinBatchSize));

    protected override void BuildModelExternal(ModelBuilder modelBuilder)
    {
        base.BuildModelExternal(modelBuilder);

        modelBuilder.Entity<Chassis>().ToTable("Chassis");
        modelBuilder.Entity<Team>().ToTable("Teams").Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<Driver>().ToTable("Drivers");
        modelBuilder.Entity<Engine>().ToTable("Engines");
        modelBuilder.Entity<EngineSupplier>().ToTable("EngineSuppliers");
        modelBuilder.Entity<Gearbox>().ToTable("Gearboxes");
        modelBuilder.Entity<Sponsor>().ToTable("Sponsors");
    }
}
