// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Data.Entity.SqlServer.FunctionalTests.TestModels;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class MappingQueryFixture : MappingQueryFixtureBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqlServerTestStore _testDatabase;

        public MappingQueryFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _testDatabase = SqlServerNorthwindContext.GetSharedStoreAsync().Result;

            _options = new DbContextOptions()
                .UseModel(CreateModel());
            _options.UseSqlServer(_testDatabase.Connection.ConnectionString);
        }

        public DbContext CreateContext()
        {
            return new DbContext(_serviceProvider, _options);
        }

        public void Dispose()
        {
            _testDatabase.Dispose();
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
