// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos;

public class MaterializationInterceptionCosmosTest : MaterializationInterceptionTestBase<MaterializationInterceptionCosmosTest.CosmosLibraryContext>,
    IClassFixture<MaterializationInterceptionCosmosTest.MaterializationInterceptionCosmosFixture>
{
    public MaterializationInterceptionCosmosTest(MaterializationInterceptionCosmosFixture fixture)
        : base(fixture)
    {
    }

    public override Task Intercept_query_materialization_with_owned_types_projecting_collection(bool async)
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

    public override LibraryContext CreateContext(IEnumerable<ISingletonInterceptor> interceptors, bool inject)
        => new CosmosLibraryContext(Fixture.CreateOptions(interceptors, inject));

    public class MaterializationInterceptionCosmosFixture : SingletonInterceptorsFixtureBase
    {
        protected override string StoreName
            => "MaterializationInterception";

        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<ISingletonInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkCosmos(), injectedInterceptors);
    }
}
