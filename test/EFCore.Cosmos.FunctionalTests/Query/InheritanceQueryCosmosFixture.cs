// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.InheritanceModel;

namespace Microsoft.EntityFrameworkCore.Query;

#nullable disable

public class InheritanceQueryCosmosFixture : InheritanceQueryFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

    public override bool UseGeneratedKeys
        => false;

    public override bool EnableComplexTypes
        => false;

    public Task NoSyncTest(bool async, Func<bool, Task> testCode)
        => CosmosTestHelpers.Instance.NoSyncTest(async, testCode);

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(
            builder.ConfigureWarnings(
                w => w.Ignore(CoreEventId.MappedEntityTypeIgnoredWarning, CosmosEventId.NoPartitionKeyDefined)));

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Animal>().ToContainer("Animals");
        modelBuilder.Entity<Plant>().ToContainer("Plants");
        modelBuilder.Entity<Plant>().Property<string>("Discriminator").ToJsonProperty("_type");
        modelBuilder.Entity<Country>().ToContainer("Countries");
        modelBuilder.Entity<Drink>().ToContainer("Drinks");
        modelBuilder.Entity<KiwiQuery>().ToContainer("Animals");
        modelBuilder.Entity<AnimalQuery>().ToContainer("Animals");
        modelBuilder.Entity<BirdQuery>().ToContainer("Animals");
        modelBuilder.Entity<KiwiQuery>().ToContainer("Animals");
    }
}
