// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class FindSqliteTest(FindSqliteTest.FindSqliteFixture fixture) : FindTestBase<FindSqliteTest.FindSqliteFixture>(fixture)
{
    public class FindSqliteTestSet(FindSqliteFixture fixture) : FindSqliteTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaSetFinder();
    }

    public class FindSqliteTestContext(FindSqliteFixture fixture) : FindSqliteTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaContextFinder();
    }

    public class FindSqliteTestNonGeneric(FindSqliteFixture fixture) : FindSqliteTest(fixture)
    {
        protected override TestFinder Finder { get; } = new FindViaNonGenericContextFinder();
    }

    public class FindSqliteFixture : FindFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<IntKey>(
                b =>
                {
                    // This configuration for SQLite prevents attempts to use the default composite key config, which doesn't work
                    // on SQLite. See #26708
                    b.OwnsOne(
                        e => e.OwnedReference, b =>
                        {
                            b.OwnsOne(e => e.NestedOwned);
                            b.OwnsMany(e => e.NestedOwnedCollection).ToTable("NestedOwnedCollection").HasKey(e => e.Prop);
                        });

                    b.OwnsMany(
                        e => e.OwnedCollection, b =>
                        {
                            b.ToTable("OwnedCollection").HasKey(e => e.Prop);
                            b.OwnsOne(e => e.NestedOwned);
                            b.OwnsMany(e => e.NestedOwnedCollection).ToTable("OwnedNestedOwnedCollection").HasKey(e => e.Prop);
                        });
                });
        }
    }
}
