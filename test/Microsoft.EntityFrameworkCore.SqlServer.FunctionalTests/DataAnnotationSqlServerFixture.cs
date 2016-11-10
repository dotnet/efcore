// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class DataAnnotationSqlServerFixture : DataAnnotationFixtureBase<SqlServerTestStore>
    {
        public static readonly string DatabaseName = "DataAnnotations";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = SqlServerTestStore.CreateConnectionString(DatabaseName);

        public DataAnnotationSqlServerFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override SqlServerTestStore CreateTestStore()
            => SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseSqlServer(_connectionString, b => b.ApplyConfiguration())
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new DataAnnotationContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureCreated();
                        DataAnnotationModelInitializer.Seed(context);

                        TestSqlLoggerFactory.Reset();
                    }
                });

        public override DataAnnotationContext CreateContext(SqlServerTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseSqlServer(testStore.Connection, b => b.ApplyConfiguration())
                .UseInternalServiceProvider(_serviceProvider)
                .ConfigureWarnings(w =>
                    {
                        w.Default(WarningBehavior.Throw);
                        w.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning);
                    });

            var context = new DataAnnotationContext(optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }
    }
}
