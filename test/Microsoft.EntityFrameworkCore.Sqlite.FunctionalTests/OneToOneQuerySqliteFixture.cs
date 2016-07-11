// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class OneToOneQuerySqliteFixture : OneToOneQueryFixtureBase, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly SqliteTestStore _testStore;

        public OneToOneQuerySqliteFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _testStore = SqliteTestStore.CreateScratch();

            _options = new DbContextOptionsBuilder()
                .UseSqlite(_testStore.ConnectionString)
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using (var context = new DbContext(_options))
            {
                context.Database.EnsureClean();

                AddTestData(context);
            }
        }

        public DbContext CreateContext() => new DbContext(_options);

        public void Dispose() => _testStore.Dispose();
    }
}
