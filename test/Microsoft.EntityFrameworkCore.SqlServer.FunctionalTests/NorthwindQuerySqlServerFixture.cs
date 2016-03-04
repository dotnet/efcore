// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.TestModels;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class NorthwindQuerySqlServerFixture : NorthwindQueryRelationalFixture, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;

        private readonly SqlServerTestStore _testStore = SqlServerNorthwindContext.GetSharedStore();
        private readonly TestSqlLoggerFactory _testSqlLoggerFactory = new TestSqlLoggerFactory();

        public NorthwindQuerySqlServerFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection()
                    .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(_testSqlLoggerFactory)
                    .BuildServiceProvider();

            _options = BuildOptions();
        }

        protected DbContextOptions BuildOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            var sqlServerDbContextOptionsBuilder
                = optionsBuilder
                    .EnableSensitiveDataLogging()
                    .UseSqlServer(_testStore.ConnectionString);

            ConfigureOptions(sqlServerDbContextOptionsBuilder);

            sqlServerDbContextOptionsBuilder.ApplyConfiguration();

            return optionsBuilder.Options;
        }

        protected virtual void ConfigureOptions(SqlServerDbContextOptionsBuilder sqlServerDbContextOptionsBuilder)
        {
        }

        public override NorthwindContext CreateContext() 
            => new SqlServerNorthwindContext(_serviceProvider, _options);

        public void Dispose() => _testStore.Dispose();

        public override CancellationToken CancelQuery() => _testSqlLoggerFactory.CancelQuery();
    }
}
