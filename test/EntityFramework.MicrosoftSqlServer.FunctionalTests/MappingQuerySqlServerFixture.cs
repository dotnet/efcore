// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class MappingQuerySqlServerFixture : MappingQueryFixtureBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqlServerTestStore _testDatabase;

        public MappingQuerySqlServerFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _testDatabase = SqlServerNorthwindContext.GetSharedStore();

            var optionsBuilder = new DbContextOptionsBuilder().UseModel(CreateModel());
            optionsBuilder.UseSqlServer(_testDatabase.Connection.ConnectionString);
            _options = optionsBuilder.Options;
        }

        public DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _options);
        }

        public void Dispose()
        {
            _testDatabase.Dispose();
        }

        protected override string DatabaseSchema { get; } = "dbo";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MappingQueryTestBase.MappedCustomer>(e =>
                {
                    e.Property(c => c.CompanyName2).Metadata.SqlServer().ColumnName = "CompanyName";
                    e.Metadata.SqlServer().TableName = "Customers";
                    e.Metadata.SqlServer().Schema = "dbo";
                });
        }
    }
}
