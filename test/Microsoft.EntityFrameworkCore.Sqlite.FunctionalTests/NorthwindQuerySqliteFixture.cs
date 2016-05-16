// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests.TestModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class NorthwindQuerySqliteFixture : NorthwindQueryRelationalFixture, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DbContextOptions _options;

        private readonly SqliteTestStore _testStore = SqliteNorthwindContext.GetSharedStore();
        private readonly TestSqlLoggerFactory _testSqlLoggerFactory = new TestSqlLoggerFactory();

        public NorthwindQuerySqliteFixture()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(_testSqlLoggerFactory)
                    .BuildServiceProvider();

            _options = BuildOptions();

            _serviceProvider.GetRequiredService<ILoggerFactory>();
        }

        protected DbContextOptions BuildOptions()
            => ConfigureOptions(
                new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(_serviceProvider))
                .UseSqlite(
                    _testStore.ConnectionString,
                    b => ConfigureOptions(b).SuppressForeignKeyEnforcement())
                .Options;

        protected virtual DbContextOptionsBuilder ConfigureOptions(DbContextOptionsBuilder dbContextOptionsBuilder)
            => dbContextOptionsBuilder;

        protected virtual SqliteDbContextOptionsBuilder ConfigureOptions(SqliteDbContextOptionsBuilder sqliteDbContextOptionsBuilder)
            => sqliteDbContextOptionsBuilder;

        public override NorthwindContext CreateContext()
            => new SqliteNorthwindContext(_options);

        public void Dispose() => _testStore.Dispose();

        public override CancellationToken CancelQuery() => _testSqlLoggerFactory.CancelQuery();
    }
}
