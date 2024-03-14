// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class MaterializationInterceptionCosmosTest :
    MaterializationInterceptionTestBase<MaterializationInterceptionCosmosTest.CosmosLibraryContext>
{
    public override Task Intercept_query_materialization_with_owned_types_projecting_collection(bool async, bool usePooling)
        => Task.CompletedTask;

    public override Task Intercept_query_materialization_with_owned_types(bool async, bool usePooling)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Intercept_query_materialization_with_owned_types(a, usePooling));

    public class CosmosLibraryContext(DbContextOptions options) : LibraryContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntity30244>();
        }
    }

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;
}
