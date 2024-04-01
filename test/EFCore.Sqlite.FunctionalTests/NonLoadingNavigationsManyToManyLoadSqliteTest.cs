// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class NonLoadingNavigationsManyToManyLoadSqliteTest(NonLoadingNavigationsManyToManyLoadSqliteTest.NonLoadingNavigationsManyToManyLoadSqliteFixture fixture)
    : ManyToManyLoadTestBase<NonLoadingNavigationsManyToManyLoadSqliteTest.NonLoadingNavigationsManyToManyLoadSqliteFixture>(fixture)
{
    public class NonLoadingNavigationsManyToManyLoadSqliteFixture : ManyToManyLoadFixtureBase, ITestSqlLoggerFactory
    {
        protected override string StoreName
            => "NonLoadingNavigationsManyToMany";

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).UseLazyLoadingProxies();

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<EntityOne>(
                b =>
                {
                    b.Navigation(e => e.Reference).EnableLazyLoading(false);
                    b.Navigation(e => e.Collection).EnableLazyLoading(false);
                    b.Navigation(e => e.TwoSkip).EnableLazyLoading(false);
                    b.Navigation(e => e.ThreeSkipPayloadFull).EnableLazyLoading(false);
                    b.Navigation(e => e.TwoSkipShared).EnableLazyLoading(false);
                    b.Navigation(e => e.ThreeSkipPayloadFullShared).EnableLazyLoading(false);
                    b.Navigation(e => e.JoinThreePayloadFullShared).EnableLazyLoading(false);
                    b.Navigation(e => e.SelfSkipPayloadLeft).EnableLazyLoading(false);
                    b.Navigation(e => e.JoinSelfPayloadLeft).EnableLazyLoading(false);
                    b.Navigation(e => e.SelfSkipPayloadRight).EnableLazyLoading(false);
                    b.Navigation(e => e.JoinSelfPayloadRight).EnableLazyLoading(false);
                    b.Navigation(e => e.BranchSkip).EnableLazyLoading(false);
                });

            modelBuilder.Entity<EntityCompositeKey>(
                b =>
                {
                    b.Navigation(e => e.TwoSkipShared).EnableLazyLoading(false);
                    b.Navigation(e => e.ThreeSkipFull).EnableLazyLoading(false);
                    b.Navigation(e => e.JoinThreeFull).EnableLazyLoading(false);
                    b.Navigation(e => e.RootSkipShared).EnableLazyLoading(false);
                    b.Navigation(e => e.LeafSkipFull).EnableLazyLoading(false);
                    b.Navigation(e => e.JoinLeafFull).EnableLazyLoading(false);
                });
        }
    }
}
