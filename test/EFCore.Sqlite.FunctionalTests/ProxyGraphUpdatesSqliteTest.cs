// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
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
                public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
                protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;
            }
        }

        public class LazyLoading : ProxyGraphUpdatesSqliteTestBase<LazyLoading.ProxyGraphUpdatesWithLazyLoadingSqliteFixture>
        {
            public LazyLoading(ProxyGraphUpdatesWithLazyLoadingSqliteFixture fixture)
                : base(fixture)
            {
            }

            public class ProxyGraphUpdatesWithLazyLoadingSqliteFixture : ProxyGraphUpdatesSqliteFixtureBase
            {
                protected override string StoreName { get; } = "ProxyGraphLazyLoadingUpdatesTest";

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                    => base.AddOptions(builder.UseLazyLoadingProxies());

                protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                    => base.AddServices(serviceCollection.AddEntityFrameworkProxies());
            }
        }
    }
}
