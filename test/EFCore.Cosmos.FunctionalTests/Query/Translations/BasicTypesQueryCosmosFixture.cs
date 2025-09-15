// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class BasicTypesQueryCosmosFixture : BasicTypesQueryFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(o => o.Ignore(CosmosEventId.NoPartitionKeyDefined));

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<BasicTypesEntity>(builder =>
        {
            builder.ToContainer(nameof(BasicTypesEntity));
            builder.HasPartitionKey(b => b.Id);
        });
        modelBuilder.Entity<NullableBasicTypesEntity>(builder =>
        {
            builder.ToContainer(nameof(NullableBasicTypesEntity));
            builder.HasPartitionKey(n => n.Id);
        });
    }

    public Task NoSyncTest(bool async, Func<bool, Task> testCode)
        => CosmosTestHelpers.Instance.NoSyncTest(async, testCode);
}
