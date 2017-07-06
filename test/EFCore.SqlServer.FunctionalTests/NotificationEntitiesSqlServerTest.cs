// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
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
                        .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider(validateScopes: true))
                    .Options;
            }

            public override DbContext CreateContext()
                => new DbContext(_options);

            public override SqlServerTestStore CreateTestStore()
                => SqlServerTestStore.GetOrCreateShared(DatabaseName, EnsureCreated);
        }
    }
}
