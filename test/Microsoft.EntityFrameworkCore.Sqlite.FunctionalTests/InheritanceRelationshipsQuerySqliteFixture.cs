// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.InheritanceRelationships;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class InheritanceRelationshipsQuerySqliteFixture : InheritanceRelationshipsQueryRelationalFixture<SqliteTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public InheritanceRelationshipsQuerySqliteFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override InheritanceRelationshipsContext CreateContext(SqliteTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseSqlite(testStore.Connection)
                .UseInternalServiceProvider(_serviceProvider);

            var context = new InheritanceRelationshipsContext(optionsBuilder.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        public override SqliteTestStore CreateTestStore()
            => SqliteTestStore.GetOrCreateShared(
                nameof(InheritanceRelationshipsQuerySqliteTest),
                () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder()
                            .UseSqlite(SqliteTestStore.CreateConnectionString(nameof(InheritanceRelationshipsQuerySqliteTest)))
                            .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new InheritanceRelationshipsContext(optionsBuilder.Options))
                    {
                        // TODO: Delete DB if model changed
                        context.Database.EnsureDeleted();
                        if (context.Database.EnsureCreated())
                        {
                            InheritanceRelationshipsModelInitializer.Seed(context);
                        }

                        TestSqlLoggerFactory.SqlStatements.Clear();
                    }
                });
    }
}