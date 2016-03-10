// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class MigrationsSqlServerFixture : MigrationsFixtureBase
    {
        private readonly DbContextOptions _options;

        public MigrationsSqlServerFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .BuildServiceProvider();

            var connectionStringBuilder = new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection)
            {
                InitialCatalog = nameof(MigrationsSqlServerTest)
            };

            _options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider)
                .UseSqlServer(connectionStringBuilder.ConnectionString).Options;
        }

        public override MigrationsContext CreateContext() => new MigrationsContext(_options);
    }
}
