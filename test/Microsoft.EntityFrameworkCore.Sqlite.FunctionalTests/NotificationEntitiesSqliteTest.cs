// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class NotificationEntitiesSqliteTest
        : NotificationEntitiesTestBase<SqliteTestStore, NotificationEntitiesSqliteTest.NotificationEntitiesSqliteFixture>
    {
        public NotificationEntitiesSqliteTest(NotificationEntitiesSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesSqliteFixture : NotificationEntitiesFixtureBase
        {
            public static readonly string DatabaseName = "NotificationEntities";
            private readonly DbContextOptions _options;

            public NotificationEntitiesSqliteFixture()
            {
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                _options = new DbContextOptionsBuilder()
                    .UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName))
                    .UseInternalServiceProvider(serviceProvider)
                    .Options;
            }

            public override DbContext CreateContext()
                => new DbContext(_options);

            public override SqliteTestStore CreateTestStore()
                => SqliteTestStore.GetOrCreateShared(DatabaseName, EnsureCreated);

            protected override void EnsureCreated()
            {
                CreateContext().Database.EnsureClean();
                base.EnsureCreated();
            }
        }
    }
}
