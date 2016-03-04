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
            private readonly IServiceProvider _serviceProvider;
            private readonly DbContextOptions _options;

            public NotificationEntitiesSqlServerFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString("NotificationEntities"));
                _options = optionsBuilder.Options;

                EnsureCreated();
            }

            public override DbContext CreateContext() 
                => new DbContext(_serviceProvider, _options);
        }
    }
}
