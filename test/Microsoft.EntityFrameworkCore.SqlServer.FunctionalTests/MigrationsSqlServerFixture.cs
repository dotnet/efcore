// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class MigrationsSqlServerFixture : MigrationsFixtureBase, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly SqlServerTestStore _testStore;

        public MigrationsSqlServerFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .BuildServiceProvider();

            _testStore = SqlServerTestStore.CreateScratch();

            _options = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider)
                .UseSqlServer(_testStore.ConnectionString, b => b.ApplyConfiguration()).Options;
        }

        public override MigrationsContext CreateContext()
        {
            var context = new MigrationsContext(_options);
            context.Database.EnsureCreated();
            return context;
        }

        public override EmptyMigrationsContext CreateEmptyContext() => new EmptyMigrationsContext(_options);

        public void Dispose() => _testStore.Dispose();
    }
}
