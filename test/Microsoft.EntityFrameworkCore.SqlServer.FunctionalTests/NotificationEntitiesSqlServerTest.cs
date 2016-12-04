// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class NotificationEntitiesSqlServerTest
        : NotificationEntitiesTestBase<SqlServerTestStore, NotificationEntitiesSqlServerTest.NotificationEntitiesSqlServerFixture>
    {
        public NotificationEntitiesSqlServerTest(NotificationEntitiesSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesSqlServerFixture : NotificationEntitiesFixtureBase
        {
            public const string DatabaseName = "NotificationEntities";

            private readonly DbContextOptions _options;

            public NotificationEntitiesSqlServerFixture()
            {
                _options = new DbContextOptionsBuilder()
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName), b => b.ApplyConfiguration())
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkSqlServer()
                        .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider())
                    .Options;
            }

            public override DbContext CreateContext()
                => new DbContext(_options);

            public override SqlServerTestStore CreateTestStore()
                => SqlServerTestStore.GetOrCreateShared(DatabaseName, EnsureCreated);
        }
    }
}
