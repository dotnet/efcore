// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.NullSemanticsModel;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class NullSemanticsQuerySqlServerFixture : NullSemanticsQueryRelationalFixture<SqlServerTestStore>
    {
        public static readonly string DatabaseName = "NullSemanticsQueryTest";

        private readonly DbContextOptions _options;

        private readonly string _connectionString = SqlServerTestStore.CreateConnectionString(DatabaseName);

        public NullSemanticsQuerySqlServerFixture()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();

            _options = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(serviceProvider)
                .Options;
        }

        public override SqlServerTestStore CreateTestStore()
        {
            return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    using (var context = new NullSemanticsContext(new DbContextOptionsBuilder(_options)
                        .UseSqlServer(_connectionString, b => b.ApplyConfiguration()).Options))
                    {
                        context.Database.EnsureCreated();
                        NullSemanticsModelInitializer.Seed(context);

                        TestSqlLoggerFactory.Reset();
                    }
                });
        }

        public override NullSemanticsContext CreateContext(SqlServerTestStore testStore, bool useRelationalNulls)
        {
            var options = new DbContextOptionsBuilder(_options)
                .UseSqlServer(
                    testStore.Connection,
                    b =>
                        {
                            b.ApplyConfiguration();
                            if (useRelationalNulls)
                            {
                                b.UseRelationalNulls();
                            }
                        }).Options;

            var context = new NullSemanticsContext(options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
