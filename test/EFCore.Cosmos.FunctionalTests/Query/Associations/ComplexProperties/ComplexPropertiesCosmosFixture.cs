// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query.Associations.ComplexProperties;

public class ComplexPropertiesCosmosFixture : ComplexPropertiesFixtureBase
{
    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder)
            .ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined).Ignore(CoreEventId.MappedEntityTypeIgnoredWarning));

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Ignore<RootReferencingEntity>();

        modelBuilder.Entity<RootEntity>()
            .ToContainer("RootEntities");

        modelBuilder.Entity<ValueRootEntity>()
            .ToContainer("ValueRootEntities");
    }
}
