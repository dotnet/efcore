// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable RCS1102 // Make class static.
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ProxyGraphUpdatesSqliteTest
{
    public abstract class ProxyGraphUpdatesSqliteTestBase<TFixture> : ProxyGraphUpdatesTestBase<TFixture>
        where TFixture : ProxyGraphUpdatesSqliteTestBase<TFixture>.ProxyGraphUpdatesSqliteFixtureBase, new()
    {
        protected ProxyGraphUpdatesSqliteTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public abstract class ProxyGraphUpdatesSqliteFixtureBase : ProxyGraphUpdatesFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;
        }
    }

    public class LazyLoading(LazyLoading.ProxyGraphUpdatesWithLazyLoadingSqliteFixture fixture)
        : ProxyGraphUpdatesSqliteTestBase<LazyLoading.ProxyGraphUpdatesWithLazyLoadingSqliteFixture>(fixture)
    {
        protected override bool DoesLazyLoading
            => true;

        protected override bool DoesChangeTracking
            => false;

        public class ProxyGraphUpdatesWithLazyLoadingSqliteFixture : ProxyGraphUpdatesSqliteFixtureBase
        {
            protected override string StoreName
                => "ProxyGraphLazyLoadingUpdatesTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.UseLazyLoadingProxies());

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
        }
    }

    public class ChangeTracking(ChangeTracking.ProxyGraphUpdatesWithChangeTrackingSqliteFixture fixture)
        : ProxyGraphUpdatesSqliteTestBase<ChangeTracking.ProxyGraphUpdatesWithChangeTrackingSqliteFixture>(fixture)
    {
        // Needs lazy loading
        public override Task Save_two_entity_cycle_with_lazy_loading()
            => Task.CompletedTask;

        protected override bool DoesLazyLoading
            => false;

        protected override bool DoesChangeTracking
            => true;

        public class ProxyGraphUpdatesWithChangeTrackingSqliteFixture : ProxyGraphUpdatesSqliteFixtureBase
        {
            protected override string StoreName
                => "ProxyGraphChangeTrackingUpdatesTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.UseChangeTrackingProxies());

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
        }
    }

    public class ChangeTrackingAndLazyLoading(
        ChangeTrackingAndLazyLoading.ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingSqliteFixture fixture)
        : ProxyGraphUpdatesSqliteTestBase<
            ChangeTrackingAndLazyLoading.ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingSqliteFixture>(fixture)
    {
        protected override bool DoesLazyLoading
            => true;

        protected override bool DoesChangeTracking
            => true;

        public class ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingSqliteFixture : ProxyGraphUpdatesSqliteFixtureBase
        {
            protected override string StoreName
                => "ProxyGraphChangeTrackingAndLazyLoadingUpdatesTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.UseChangeTrackingProxies().UseLazyLoadingProxies());

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
        }
    }
}
