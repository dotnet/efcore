// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class MigrationsSqliteFixture : MigrationsFixtureBase
    {
        protected override void ConfigureServices(IServiceCollection services)
        {
            services.AddEntityFramework().AddSqlite();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=" + nameof(MigrationsSqliteTest) + ".db");
        }
    }
}
