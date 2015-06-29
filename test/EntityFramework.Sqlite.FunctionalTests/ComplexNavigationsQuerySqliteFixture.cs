// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ComplexNavigationsModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class ComplexNavigationsQuerySqliteFixture : ComplexNavigationsQueryRelationalFixture<SqliteTestStore>
    {
        public static readonly string DatabaseName = "ComplexNavigations";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = SqliteTestStore.CreateConnectionString(DatabaseName);

        public ComplexNavigationsQuerySqliteFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override SqliteTestStore CreateTestStore() =>
            SqliteTestStore.GetOrCreateShared(
                DatabaseName,
                () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder();
                        optionsBuilder.UseSqlite(_connectionString);

                        using (var context = new ComplexNavigationsContext(_serviceProvider, optionsBuilder.Options))
                        {
                            if (context.Database.EnsureCreated())
                            {
                                ComplexNavigationsModelInitializer.Seed(context);
                            }

                            TestSqlLoggerFactory.SqlStatements.Clear();
                        }
                    });

        public override ComplexNavigationsContext CreateContext(SqliteTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite(testStore.Connection);

            var context = new ComplexNavigationsContext(_serviceProvider, optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
