// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Data.Entity.SQLite.FunctionalTests.TestModels;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class SQLiteNorthwindQueryFixture : RelationalNorthwindQueryFixture, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SQLiteTestStore _testStore;

        public SQLiteNorthwindQueryFixture()
        {
            _testStore = SQLiteNorthwindContext.GetSharedStoreAsync().Result;

            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSQLite()
                    .UseLoggerFactory(new TestSqlLoggerFactory())
                    .ServiceCollection
                    .BuildServiceProvider();

            _options
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseSQLite(_testStore.Connection.ConnectionString);
        }

        public override NorthwindContext CreateContext()
        {
            return new SQLiteNorthwindContext(_serviceProvider, _options);
        }

        public void Dispose()
        {
            _testStore.Dispose();
        }
    }
}
