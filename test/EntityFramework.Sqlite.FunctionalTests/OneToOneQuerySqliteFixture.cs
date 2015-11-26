// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class OneToOneQuerySqliteFixture : OneToOneQueryFixtureBase, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly IServiceProvider _serviceProvider;
        private readonly SqliteTestStore _testStore;

        public OneToOneQuerySqliteFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlite()
                    .ServiceCollection()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                    .BuildServiceProvider();

            _testStore = SqliteTestStore.CreateScratch();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite(_testStore.Connection.ConnectionString);
            _options = optionsBuilder.Options;

            using (var context = new DbContext(_serviceProvider, _options))
            {
                context.Database.EnsureCreated();

                AddTestData(context);
            }
        }

        public DbContext CreateContext() => new DbContext(_serviceProvider, _options);

        public void Dispose() => _testStore.Dispose();
    }
}
