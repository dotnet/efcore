// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class MaterializationInterceptionInMemoryTest : MaterializationInterceptionTestBase<MaterializationInterceptionInMemoryTest.InMemoryLibraryContext>,
    IClassFixture<MaterializationInterceptionInMemoryTest.MaterializationInterceptionInMemoryFixture>
{
    public MaterializationInterceptionInMemoryTest(MaterializationInterceptionInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public class InMemoryLibraryContext : LibraryContext
    {
        public InMemoryLibraryContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntity30244>().OwnsMany(e => e.Settings);
        }
    }

    public override LibraryContext CreateContext(IEnumerable<ISingletonInterceptor> interceptors, bool inject)
        => new InMemoryLibraryContext(Fixture.CreateOptions(interceptors, inject));

    public class MaterializationInterceptionInMemoryFixture : SingletonInterceptorsFixtureBase
    {
        protected override string StoreName
            => "MaterializationInterception";

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<ISingletonInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkInMemoryDatabase(), injectedInterceptors);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    }
}
