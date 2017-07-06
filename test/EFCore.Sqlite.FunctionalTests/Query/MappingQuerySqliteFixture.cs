// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class MappingQuerySqliteFixture : MappingQueryFixtureBase
    {
        private readonly DbContextOptions _options;
        private readonly SqliteTestStore _testDatabase;

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public MappingQuerySqliteFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider(validateScopes: true);

            _testDatabase = SqliteTestStore.GetNorthwindStore();

            _options = new DbContextOptionsBuilder()
                .UseModel(CreateModel())
                .UseSqlite(
                    _testDatabase.ConnectionString,
                    b => b.SuppressForeignKeyEnforcement())
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

        protected override string DatabaseSchema { get; } = null;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MappingQueryTestBase.MappedCustomer>(e =>
                {
                    // TODO: Use .Sqlite() when available
                    e.Property(c => c.CompanyName2).Metadata.Relational().ColumnName = "CompanyName";
                    e.Metadata.Relational().TableName = "Customers";
                });
        }
    }
}
