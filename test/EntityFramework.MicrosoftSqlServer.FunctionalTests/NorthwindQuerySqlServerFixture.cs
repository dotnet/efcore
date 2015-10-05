// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
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
                    .AddInstance<ILoggerFactory>(_testSqlLoggerFactory)
                    .BuildServiceProvider();

            _options = BuildOptions();
        }

        protected DbContextOptions BuildOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            var sqlServerDbContextOptionsBuilder
                = optionsBuilder.UseSqlServer(_testStore.Connection.ConnectionString);

            sqlServerDbContextOptionsBuilder.LogSqlParameterValues();

            ConfigureOptions(sqlServerDbContextOptionsBuilder);

            sqlServerDbContextOptionsBuilder.ApplyConfiguration();

            return optionsBuilder.Options;
        }

        protected virtual void ConfigureOptions(SqlServerDbContextOptionsBuilder sqlServerDbContextOptionsBuilder)
        {
        }

        public override NorthwindContext CreateContext()
        {
            var context = new SqlServerNorthwindContext(_serviceProvider, _options);

            context.ChangeTracker.AutoDetectChangesEnabled = false;

            return context;
        }

        public void Dispose() => _testStore.Dispose();

        public override CancellationToken CancelQuery() => _testSqlLoggerFactory.CancelQuery();
    }
}
