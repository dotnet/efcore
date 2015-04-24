// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Data.Entity.Sqlite.FunctionalTests.TestModels;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class NorthwindQuerySqliteFixture : NorthwindQueryRelationalFixture, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqliteTestStore _testStore;

        public NorthwindQuerySqliteFixture()
        {
            _testStore = SqliteNorthwindContext.GetSharedStore();

            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite(_testStore.Connection.ConnectionString);
            _options = optionsBuilder.Options;

            _serviceProvider.GetRequiredService<ILoggerFactory>()
                .MinimumLevel = LogLevel.Debug;
        }

        public override NorthwindContext CreateContext() => new SqliteNorthwindContext(_serviceProvider, _options);

        public void Dispose() => _testStore.Dispose();
    }
}
