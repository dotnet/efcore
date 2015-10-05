// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
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

            var connectionStringBuilder = new SqlConnectionStringBuilder
            {
                InitialCatalog = nameof(MigrationsSqlServerTest)
            };
            connectionStringBuilder.ApplyConfiguration();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlServer(connectionStringBuilder.ConnectionString);
            _options = optionsBuilder.Options;
        }

        public override MigrationsContext CreateContext() => new MigrationsContext(_serviceProvider, _options);
    }
}
