// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class MaterializationInterceptionSqliteTest : MaterializationInterceptionTestBase<MaterializationInterceptionSqliteTest.SqliteLibraryContext>,
    IClassFixture<MaterializationInterceptionSqliteTest.MaterializationInterceptionSqliteFixture>
{
    public MaterializationInterceptionSqliteTest(MaterializationInterceptionSqliteFixture fixture)
        : base(fixture)
    {
    }

    public class SqliteLibraryContext : LibraryContext
    {
        public SqliteLibraryContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntity30244>().OwnsMany(e => e.Settings, b => b.ToJson());
        }
    }

    public override LibraryContext CreateContext(IEnumerable<ISingletonInterceptor> interceptors, bool inject)
        => new SqliteLibraryContext(Fixture.CreateOptions(interceptors, inject));

    public class MaterializationInterceptionSqliteFixture : SingletonInterceptorsFixtureBase
    {
        protected override string StoreName
            => "MaterializationInterception";

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<ISingletonInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlite(), injectedInterceptors);
    }
}
