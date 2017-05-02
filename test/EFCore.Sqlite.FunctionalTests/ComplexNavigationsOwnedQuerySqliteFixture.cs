// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class ComplexNavigationsOwnedQuerySqliteFixture : ComplexNavigationsOwnedQueryFixtureBase<SqliteTestStore>
    {
        public static readonly string DatabaseName = "ComplexNavigationsOwned";

        private readonly DbContextOptions _options;
        private readonly string _connectionString = SqliteTestStore.CreateConnectionString(DatabaseName);

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public ComplexNavigationsOwnedQuerySqliteFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .UseSqlite(_connectionString)
                .UseInternalServiceProvider(serviceProvider)
                .EnableSensitiveDataLogging()
                .Options;
        }

        public override SqliteTestStore CreateTestStore() =>
            SqliteTestStore.GetOrCreateShared(
                DatabaseName,
                () =>
                    {
                        using (var context = new ComplexNavigationsContext(_options))
                        {
                            context.Database.EnsureClean();
                            ComplexNavigationsModelInitializer.Seed(context, tableSplitting: true);
                        }
                    });

        public override ComplexNavigationsContext CreateContext(SqliteTestStore testStore)
        {
            var context = new ComplexNavigationsContext(_options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
