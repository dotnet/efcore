// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore;

public class MaterializationInterceptionInMemoryTest :
    MaterializationInterceptionTestBase<MaterializationInterceptionInMemoryTest.InMemoryLibraryContext>
{
    public class InMemoryLibraryContext(DbContextOptions options) : LibraryContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntity30244>().OwnsMany(e => e.Settings);
        }
    }

    protected override ITestStoreFactory TestStoreFactory
        => InMemoryTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(c => c.Ignore(InMemoryEventId.TransactionIgnoredWarning));
}
