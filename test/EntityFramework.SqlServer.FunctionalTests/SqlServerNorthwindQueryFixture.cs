// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerNorthwindQueryFixture : RelationalNorthwindQueryFixture, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqlServerTestStore _testStore;

        public SqlServerNorthwindQueryFixture()
        {
            _testStore = SqlServerNorthwindContext.GetSharedStoreAsync().Result;

            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer().ServiceCollection
                .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _options
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseSqlServer(_testStore.Connection.ConnectionString);
        }

        public override NorthwindContext CreateContext()
        {
            return new SqlServerNorthwindContext(_serviceProvider, _options);
        }

        public void Dispose()
        {
            _testStore.Dispose();
        }
    }
}
