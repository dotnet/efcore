// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class NorthwindQueryFixture : NorthwindQueryFixtureRelationalBase, IDisposable
    {
        private readonly TestSqlLoggerFactory _loggingFactory = new TestSqlLoggerFactory();

        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SQLiteTestDatabase _testDatabase;

        public NorthwindQueryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSQLite()
                    .UseLoggerFactory(_loggingFactory)
                    .ServiceCollection
                    .BuildServiceProvider();

            _testDatabase = SQLiteTestDatabase.Northwind().Result;

            _options = new DbContextOptions()
                .UseModel(SetTableNames(CreateModel()))
                .UseSQLite(_testDatabase.Connection.ConnectionString);
        }

        public DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _options);
        }

        public string Sql
        {
            get { return TestSqlLoggerFactory.Logger.Sql; }
        }

        public void Dispose()
        {
            _testDatabase.Dispose();
        }

        public void InitLogger()
        {
            _loggingFactory.Init();
        }
    }
}
