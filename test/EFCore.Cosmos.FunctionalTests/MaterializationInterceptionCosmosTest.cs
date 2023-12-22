// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class MaterializationInterceptionCosmosTest :
    MaterializationInterceptionTestBase<MaterializationInterceptionCosmosTest.CosmosLibraryContext>
{
    public override Task Intercept_query_materialization_with_owned_types_projecting_collection(bool async, bool usePooling)
        => Task.CompletedTask;

    public class CosmosLibraryContext : LibraryContext
    {
        public CosmosLibraryContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntity30244>();
        }
    }

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;
}
