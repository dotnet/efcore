// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.SqlClient;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class MigrationsSqlServerFixture : MigrationsFixtureBase
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFramework().AddSqlServer();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("" +
                new SqlConnectionStringBuilder
                {
                    DataSource = @"(localdb)\MSSQLLocalDB",
                    InitialCatalog = nameof(MigrationsSqlServerTest),
                    IntegratedSecurity = true
                });
        }
    }
}
