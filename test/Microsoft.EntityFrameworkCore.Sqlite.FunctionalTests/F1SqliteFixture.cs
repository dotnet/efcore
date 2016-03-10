// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class F1SqliteFixture : F1RelationalFixture<SqliteTestStore>
    {
        public static readonly string DatabaseName = "OptimisticConcurrencyTest";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = SqliteTestStore.CreateConnectionString(DatabaseName);

        public F1SqliteFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override SqliteTestStore CreateTestStore()
        {
            return SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseSqlite(_connectionString)
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new F1Context(optionsBuilder.Options))
                    {
                        // TODO: Delete DB if model changed
                        context.Database.EnsureDeleted();
                        if (context.Database.EnsureCreated())
                        {
                            ConcurrencyModelInitializer.Seed(context);
                        }

                        TestSqlLoggerFactory.SqlStatements.Clear();
                    }
                });
        }

        public override F1Context CreateContext(SqliteTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseSqlite(testStore.Connection)
                .UseInternalServiceProvider(_serviceProvider);

            var context = new F1Context(optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }
    }
}
