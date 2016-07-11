// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
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

        public override ModelValidator ThrowingValidator
            => new ThrowingModelValidator(
                _serviceProvider.GetService<ILogger<RelationalModelValidator>>(),
                new SqlServerAnnotationProvider(),
                new SqlServerTypeMapper());

        private class ThrowingModelValidator : RelationalModelValidator
        {
            public ThrowingModelValidator(
                ILogger<RelationalModelValidator> loggerFactory,
                IRelationalAnnotationProvider relationalExtensions,
                IRelationalTypeMapper typeMapper)
                : base(loggerFactory, relationalExtensions, typeMapper)
            {
            }

            protected override void ShowWarning(string message)
            {
                throw new InvalidOperationException(message);
            }
        }

        public override SqlServerTestStore CreateTestStore()
        {
            return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseSqlServer(_connectionString)
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new DataAnnotationContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureClean();
                        DataAnnotationModelInitializer.Seed(context);

                        TestSqlLoggerFactory.Reset();
                    }
                });
        }

        public override DataAnnotationContext CreateContext(SqlServerTestStore testStore)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseSqlServer(testStore.Connection)
                .UseInternalServiceProvider(_serviceProvider);

            var context = new DataAnnotationContext(optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }
    }
}
