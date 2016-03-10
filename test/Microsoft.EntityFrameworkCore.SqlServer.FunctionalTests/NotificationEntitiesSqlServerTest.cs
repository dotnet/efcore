// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class NotificationEntitiesSqlServerTest 
        : NotificationEntitiesTestBase<NotificationEntitiesSqlServerTest.NotificationEntitiesSqlServerFixture>
    {
        public NotificationEntitiesSqlServerTest(NotificationEntitiesSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class NotificationEntitiesSqlServerFixture : NotificationEntitiesFixtureBase
        {
            private readonly DbContextOptions _options;

            public NotificationEntitiesSqlServerFixture()
            {
                _options = new DbContextOptionsBuilder()
                    .UseSqlServer(SqlServerTestStore.CreateConnectionString("NotificationEntities"))
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkSqlServer()
                        .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider())
                    .Options;

                EnsureCreated();
            }

            public override DbContext CreateContext() 
                => new DbContext(_options);
        }
    }
}
