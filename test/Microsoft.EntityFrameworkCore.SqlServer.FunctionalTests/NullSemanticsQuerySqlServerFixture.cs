// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestModels.NullSemanticsModel;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class NullSemanticsQuerySqlServerFixture : NullSemanticsQueryRelationalFixture<SqlServerTestStore>
    {
        public static readonly string DatabaseName = "NullSemanticsQueryTest";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = SqlServerTestStore.CreateConnectionString(DatabaseName);

        public NullSemanticsQuerySqlServerFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override SqlServerTestStore CreateTestStore()
        {
            return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlServer(_connectionString);

                using (var context = new NullSemanticsContext(_serviceProvider, optionsBuilder.Options))
                {
                    // TODO: Delete DB if model changed

                    if (context.Database.EnsureCreated())
                    {
                        NullSemanticsModelInitializer.Seed(context);
                    }

                    TestSqlLoggerFactory.SqlStatements.Clear();
                }
            });
        }

        public override NullSemanticsContext CreateContext(SqlServerTestStore testStore, bool useRelationalNulls)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            var sqlServerOptions
                = optionsBuilder
                    .EnableSensitiveDataLogging()
                    .UseSqlServer(testStore.Connection);

            if (useRelationalNulls)
            {
                sqlServerOptions.UseRelationalNulls();
            }

            var context = new NullSemanticsContext(_serviceProvider, optionsBuilder.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}