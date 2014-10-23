// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class MappingQueryFixture : MappingQueryFixtureBase
    {
        private readonly TestSqlLoggerFactory _loggingFactory = new TestSqlLoggerFactory();

        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqlServerTestDatabase _testDatabase;

        public MappingQueryFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .UseLoggerFactory(_loggingFactory)
                    .ServiceCollection
                    .BuildServiceProvider();

            _testDatabase = SqlServerTestDatabase.Northwind().Result;

            _options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseSqlServer(_testDatabase.Connection.ConnectionString);
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

        protected override void OnModelCreating(BasicModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MappingQueryTestBase.MappedCustomer>(e =>
                {
                    e.Property(c => c.CompanyName2).ForSqlServer(c => c.Column("CompanyName"));
                    e.ForSqlServer(t => t.Table("Customers", "dbo"));
                });
        }
    }
}
