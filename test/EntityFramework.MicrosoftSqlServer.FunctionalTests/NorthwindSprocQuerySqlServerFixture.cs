// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class NorthwindSprocQuerySqlServerFixture : NorthwindSprocQueryRelationalFixture, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqlServerTestStore _testStore;

        public NorthwindSprocQuerySqlServerFixture()
        {
            _testStore = SqlServerNorthwindContext.GetSharedStore();

            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            var optionsBuilder = new DbContextOptionsBuilder();

            optionsBuilder
                .UseSqlServer(_testStore.Connection.ConnectionString)
                .LogSqlParameterValues();

            _options = optionsBuilder.Options;

            _serviceProvider.GetRequiredService<ILoggerFactory>()
                .MinimumLevel = LogLevel.Debug;
        }

        public override NorthwindContext CreateContext()
        {
            var context = new SqlServerNorthwindContext(_serviceProvider, _options);

            context.ChangeTracker.AutoDetectChangesEnabled = false;

            return context;
        }

        public void Dispose() => _testStore.Dispose();
    }
}
