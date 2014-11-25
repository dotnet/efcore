// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Data.Entity.Sqlite.FunctionalTests.TestModels;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class SqLiteNorthwindQueryFixture : RelationalNorthwindQueryFixture, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqLiteTestStore _testStore;

        public SqLiteNorthwindQueryFixture()
        {
            _testStore = SqliteNorthwindContext.GetSharedStoreAsync().Result;

            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection
                .AddTestModelSource(OnModelCreating)
                .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _options
                = new DbContextOptions()
                    .UseSqlite(_testStore.Connection.ConnectionString);
        }

        public override NorthwindContext CreateContext()
        {
            return new SqliteNorthwindContext(_serviceProvider, _options);
        }

        public void Dispose()
        {
            _testStore.Dispose();
        }
    }
}
