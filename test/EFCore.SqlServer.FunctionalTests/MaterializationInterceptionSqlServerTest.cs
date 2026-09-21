// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class MaterializationInterceptionSqlServerTest(NonSharedFixture fixture) :
    MaterializationInterceptionTestBase<MaterializationInterceptionSqlServerTest.SqlServerLibraryContext>(fixture)
{
    public class SqlServerLibraryContext(DbContextOptions options) : LibraryContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntity30244>().OwnsMany(e => e.Settings, b => b.ToJson());
        }
    }

    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder AddNonSharedOptions(DbContextOptionsBuilder builder)
        => base.AddNonSharedOptions(builder)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.OwnedEntityMappedToJsonCollectionWarning));
}
