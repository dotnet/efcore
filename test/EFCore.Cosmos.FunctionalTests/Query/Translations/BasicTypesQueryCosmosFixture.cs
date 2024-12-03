// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.BasicTypesModel;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

public class BasicTypesQueryCosmosFixture : BasicTypesQueryFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => builder.ConfigureWarnings(o => o.Ignore(CosmosEventId.NoPartitionKeyDefined));

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<BasicTypesEntity>().ToContainer(nameof(BasicTypesEntity));
        modelBuilder.Entity<NullableBasicTypesEntity>().ToContainer(nameof(NullableBasicTypesEntity));
    }

    public Task NoSyncTest(bool async, Func<bool, Task> testCode)
        => CosmosTestHelpers.Instance.NoSyncTest(async, testCode);
}
