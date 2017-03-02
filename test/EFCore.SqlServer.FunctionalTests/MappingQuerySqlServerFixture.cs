// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.TestModels;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class MappingQuerySqlServerFixture : MappingQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly SqlServerTestStore _testDatabase;

        public MappingQuerySqlServerFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _testDatabase = SqlServerNorthwindContext.GetSharedStore();

            _options = new DbContextOptionsBuilder()
                .UseModel(CreateModel())
                .UseSqlServer(_testDatabase.ConnectionString, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public DbContext CreateContext()
        {
            var context = new DbContext(_options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }

        public void Dispose() => _testDatabase.Dispose();

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
