// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.RelationshipsModel;

namespace Microsoft.EntityFrameworkCore.Query.Relationships;

public class OwnedJsonRelationshipsCosmosFixture : OwnedJsonRelationshipsFixtureBase
{
    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    => base.AddOptions(
        builder.ConfigureWarnings(
            w => w.Ignore(CosmosEventId.NoPartitionKeyDefined)));

    public Task NoSyncTest(bool async, Func<bool, Task> testCode)
        => CosmosTestHelpers.Instance.NoSyncTest(async, testCode);

    public void NoSyncTest(Action testCode)
        => CosmosTestHelpers.Instance.NoSyncTest(testCode);

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<RelationshipsRootEntity>()
            .ToContainer("RootEntities")
            .HasDiscriminatorInJsonId()
            .HasDiscriminator<string>("Discriminator").HasValue("Root");

        modelBuilder.Entity<RelationshipsRootEntity>()
            .ToContainer("RootEntities");
    }
}
