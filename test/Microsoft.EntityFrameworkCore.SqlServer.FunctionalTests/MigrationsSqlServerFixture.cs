// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class MigrationsSqlServerFixture : MigrationsFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;

        public MigrationsSqlServerFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider();

            var connectionStringBuilder = new SqlConnectionStringBuilder(TestEnvironment.DefaultConnection)
            {
                InitialCatalog = nameof(MigrationsSqlServerTest)
            };

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(connectionStringBuilder.ConnectionString);
            _options = optionsBuilder.Options;
        }

        public override MigrationsContext CreateContext() => new MigrationsContext(_serviceProvider, _options);
    }
}
