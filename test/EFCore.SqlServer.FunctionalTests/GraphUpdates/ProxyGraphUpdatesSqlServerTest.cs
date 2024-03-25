// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class ProxyGraphUpdatesSqlServerTest
{
    public abstract class ProxyGraphUpdatesSqlServerTestBase<TFixture> : ProxyGraphUpdatesTestBase<TFixture>
        where TFixture : ProxyGraphUpdatesSqlServerTestBase<TFixture>.ProxyGraphUpdatesSqlServerFixtureBase, new()
    {
        protected ProxyGraphUpdatesSqlServerTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public abstract class ProxyGraphUpdatesSqlServerFixtureBase : ProxyGraphUpdatesFixtureBase
        {
            public TestSqlLoggerFactory TestSqlLoggerFactory
                => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;
        }
    }

    public class LazyLoading(LazyLoading.ProxyGraphUpdatesWithLazyLoadingSqlServerFixture fixture)
        : ProxyGraphUpdatesSqlServerTestBase<LazyLoading.ProxyGraphUpdatesWithLazyLoadingSqlServerFixture>(fixture)
    {
        protected override bool DoesLazyLoading
            => true;

        protected override bool DoesChangeTracking
            => false;

        public class ProxyGraphUpdatesWithLazyLoadingSqlServerFixture : ProxyGraphUpdatesSqlServerFixtureBase
        {
            protected override string StoreName
                => "ProxyGraphLazyLoadingUpdatesTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.UseLazyLoadingProxies());

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.UseIdentityColumns();

                base.OnModelCreating(modelBuilder, context);
            }
        }
    }

    public class ChangeTracking(ChangeTracking.ProxyGraphUpdatesWithChangeTrackingSqlServerFixture fixture)
        : ProxyGraphUpdatesSqlServerTestBase<ChangeTracking.ProxyGraphUpdatesWithChangeTrackingSqlServerFixture>(fixture)
    {
        // Needs lazy loading
        public override Task Save_two_entity_cycle_with_lazy_loading()
            => Task.CompletedTask;

        protected override bool DoesLazyLoading
            => false;

        protected override bool DoesChangeTracking
            => true;

        public class ProxyGraphUpdatesWithChangeTrackingSqlServerFixture : ProxyGraphUpdatesSqlServerFixtureBase
        {
            protected override string StoreName
                => "ProxyGraphChangeTrackingUpdatesTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.UseChangeTrackingProxies());

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.UseIdentityColumns();

                base.OnModelCreating(modelBuilder, context);
            }
        }
    }

    public class ChangeTrackingAndLazyLoading(
        ChangeTrackingAndLazyLoading.ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingSqlServerFixture fixture)
        : ProxyGraphUpdatesSqlServerTestBase<
            ChangeTrackingAndLazyLoading.ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingSqlServerFixture>(fixture)
    {
        protected override bool DoesLazyLoading
            => true;

        protected override bool DoesChangeTracking
            => true;

        public class ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingSqlServerFixture : ProxyGraphUpdatesSqlServerFixtureBase
        {
            protected override string StoreName
                => "ProxyGraphChangeTrackingAndLazyLoadingUpdatesTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.UseLazyLoadingProxies().UseChangeTrackingProxies());

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.UseIdentityColumns();

                base.OnModelCreating(modelBuilder, context);
            }
        }
    }
}
