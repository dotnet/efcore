// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.UpdatesModel;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class UpdatesSqliteFixture : UpdatesFixtureBase<SqliteTestStore>
    {
        public static readonly string DatabaseName = "Updates";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = SqliteTestStore.CreateConnectionString(DatabaseName);

        public UpdatesSqliteFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();
        }

        public override SqliteTestStore CreateTestStore() =>
            SqliteTestStore.GetOrCreateShared(
                DatabaseName,
                () =>
                    {
                        using (var context = new UpdatesContext(new DbContextOptionsBuilder()
                            .UseSqlite(_connectionString)
                            .UseInternalServiceProvider(_serviceProvider).Options))
                        {
                            context.Database.EnsureClean();
                            UpdatesModelInitializer.Seed(context);

                            TestSqlLoggerFactory.Reset();
                        }
                    });

        public override UpdatesContext CreateContext(SqliteTestStore testStore)
        {
            var context = new UpdatesContext(new DbContextOptionsBuilder()
                .UseSqlite(testStore.Connection,
                    b => b.SuppressForeignKeyEnforcement())
                .UseInternalServiceProvider(_serviceProvider).Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
