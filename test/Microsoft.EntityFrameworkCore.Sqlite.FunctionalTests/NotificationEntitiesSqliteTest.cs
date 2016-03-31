// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests;
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
            private readonly IServiceProvider _serviceProvider;
            private readonly DbContextOptions _options;

            public NotificationEntitiesSqliteFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                _options = new DbContextOptionsBuilder()
                    .UseSqlite(SqliteTestStore.CreateConnectionString("NotificationEntities"))
                    .UseInternalServiceProvider(_serviceProvider)
                    .Options;

                EnsureCreated();
            }

            public override DbContext CreateContext()
                => new DbContext(_options);
        }
    }
}
