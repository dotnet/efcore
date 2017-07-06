// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class GearsOfWarQuerySqliteFixture : GearsOfWarQueryRelationalFixture<SqliteTestStore>
    {
        public static readonly string DatabaseName = "GearsOfWarQueryTest";

        private readonly DbContextOptions _options;

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public GearsOfWarQuerySqliteFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider(validateScopes: true);

            _options = new DbContextOptionsBuilder()
                .UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName))
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public override SqliteTestStore CreateTestStore()
        {
            return SqliteTestStore.GetOrCreateShared(
                DatabaseName, () =>
                    {
                        using (var context = new GearsOfWarContext(_options))
                        {
                            context.Database.EnsureClean();
                            GearsOfWarModelInitializer.Seed(context);
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
