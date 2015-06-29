// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Sqlite.FunctionalTests.TestModels;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class MappingQuerySqliteFixture : MappingQueryFixtureBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;
        private readonly SqliteTestStore _testDatabase;

        public MappingQuerySqliteFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection()
                .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _testDatabase = SqliteNorthwindContext.GetSharedStore();

            var optionsBuilder = new DbContextOptionsBuilder().UseModel(CreateModel());
            optionsBuilder.UseSqlite(_testDatabase.Connection.ConnectionString);
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

        protected override string DatabaseSchema { get; } = null;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MappingQueryTestBase.MappedCustomer>(e =>
                {
                    // TODO: Use .Sqlite() when available
                    e.Property(c => c.CompanyName2).Metadata.Relational().Column = "CompanyName";
                    e.Metadata.Relational().Table = "Customers";
                });
        }
    }
}
