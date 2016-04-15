// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class NotificationEntitiesSqliteTest
        : NotificationEntitiesTestBase<NotificationEntitiesSqliteTest.NotificationEntitiesSqliteFixture>
    {
        public NotificationEntitiesSqliteTest(NotificationEntitiesSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesSqliteFixture : NotificationEntitiesFixtureBase
        {
            private readonly DbContextOptions _options;

            public NotificationEntitiesSqliteFixture()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                _options = new DbContextOptionsBuilder()
                    .UseSqlite(SqliteTestStore.CreateConnectionString("NotificationEntities"))
                    .UseInternalServiceProvider(serviceProvider)
                    .Options;

                EnsureCreated();
            }

            public override DbContext CreateContext()
                => new DbContext(_options);
        }
    }
}
