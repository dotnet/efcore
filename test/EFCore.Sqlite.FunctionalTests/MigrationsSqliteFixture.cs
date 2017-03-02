// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class MigrationsSqliteFixture : MigrationsFixtureBase
    {
        private readonly DbContextOptions _options;

        public MigrationsSqliteFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider)
                .UseSqlite("Data Source=" + nameof(MigrationsSqliteTest) + ".db").Options;
        }

        public override MigrationsContext CreateContext()
            => new MigrationsContext(_options);

        public override EmptyMigrationsContext CreateEmptyContext()
            => new EmptyMigrationsContext(_options);
    }
}
