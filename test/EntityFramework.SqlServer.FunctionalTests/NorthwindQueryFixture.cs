// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class NorthwindQueryFixture : NorthwindQueryFixtureRelationalBase, IDisposable
    {
        private readonly TestSqlLoggerFactory _loggingFactory = new TestSqlLoggerFactory();

        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqlServerTestDatabase _testDatabase;

        public NorthwindQueryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .UseLoggerFactory(_loggingFactory)
                    .ServiceCollection
                    .BuildServiceProvider();

            _testDatabase = SqlServerTestDatabase.Northwind().Result;

            _options
                = new DbContextOptions()
                    .UseModel(SetTableNames(CreateModel()))
                    .UseSqlServer(_testDatabase.Connection.ConnectionString);
        }

        public string Sql
        {
            get { return TestSqlLoggerFactory.Logger.Sql; }
        }

        public string Log
        {
            get { return TestSqlLoggerFactory.Logger.Log; }
        }

        public void Dispose()
        {
            _testDatabase.Dispose();
        }

        public DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _options);
        }

        public DbContext CreateContext(SqlServerTestDatabase testDatabase)
        {
            var options = new DbContextOptions()
                .UseModel(SetTableNames(CreateModel()))
                .UseSqlServer(testDatabase.Connection.ConnectionString);
            return new DbContext(_serviceProvider, options);
        }

        public void InitLogger()
        {
            _loggingFactory.Init();
        }

        public CancellationToken CancelQuery()
        {
            return TestSqlLoggerFactory.Logger.CancelQuery();
        }
    }
}
