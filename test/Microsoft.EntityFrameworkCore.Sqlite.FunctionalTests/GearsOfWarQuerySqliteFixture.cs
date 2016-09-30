// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.GearsOfWarModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class GearsOfWarQuerySqliteFixture : GearsOfWarQueryRelationalFixture<SqliteTestStore>
    {
        public static readonly string DatabaseName = "GearsOfWarQueryTest";

        private readonly DbContextOptions _options;

        private readonly string _connectionString = SqliteTestStore.CreateConnectionString(DatabaseName);

        public GearsOfWarQuerySqliteFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .UseSqlite(_connectionString)
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public override SqliteTestStore CreateTestStore()
        {
            return SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    using (var context = new GearsOfWarContext(_options))
                    {
                        context.Database.EnsureClean();
                        GearsOfWarModelInitializer.Seed(context);

                        TestSqlLoggerFactory.Reset();
                    }
                });
        }

        public override GearsOfWarContext CreateContext(SqliteTestStore testStore)
        {
            var context = new GearsOfWarContext(_options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
