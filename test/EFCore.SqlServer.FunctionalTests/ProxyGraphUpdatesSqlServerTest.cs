// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
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
                public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
                protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
            }
        }

        public class LazyLoading : ProxyGraphUpdatesSqlServerTestBase<LazyLoading.ProxyGraphUpdatesWithLazyLoadingSqlServerFixture>
        {
            public LazyLoading(ProxyGraphUpdatesWithLazyLoadingSqlServerFixture fixture)
                : base(fixture)
            {
            }

            protected override bool DoesLazyLoading => true;
            protected override bool DoesChangeTracking => false;

            public class ProxyGraphUpdatesWithLazyLoadingSqlServerFixture : ProxyGraphUpdatesSqlServerFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphLazyLoadingUpdatesTest";

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

        public class ChangeTracking : ProxyGraphUpdatesSqlServerTestBase<ChangeTracking.ProxyGraphUpdatesWithChangeTrackingSqlServerFixture>
        {
            public ChangeTracking(ProxyGraphUpdatesWithChangeTrackingSqlServerFixture fixture)
                : base(fixture)
            {
            }

            protected override bool DoesLazyLoading => false;
            protected override bool DoesChangeTracking => true;

            public class ProxyGraphUpdatesWithChangeTrackingSqlServerFixture : ProxyGraphUpdatesSqlServerFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphChangeTrackingUpdatesTest";

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                    => base.AddOptions(builder.UseChangeDetectionProxies());

                protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                    => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

                protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                {
                    modelBuilder.UseIdentityColumns();

                    base.OnModelCreating(modelBuilder, context);
                }
            }
        }

        public class ChangeTrackingAndLazyLoading : ProxyGraphUpdatesSqlServerTestBase<ChangeTrackingAndLazyLoading.ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingSqlServerFixture>
        {
            public ChangeTrackingAndLazyLoading(ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingSqlServerFixture fixture)
                : base(fixture)
            {
            }

            protected override bool DoesLazyLoading => true;
            protected override bool DoesChangeTracking => true;

            public class ProxyGraphUpdatesWithChangeTrackingAndLazyLoadingSqlServerFixture : ProxyGraphUpdatesSqlServerFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphChangeTrackingAndLazyLoadingUpdatesTest";

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                    => base.AddOptions(builder.UseLazyLoadingProxies().UseChangeDetectionProxies());

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
}
