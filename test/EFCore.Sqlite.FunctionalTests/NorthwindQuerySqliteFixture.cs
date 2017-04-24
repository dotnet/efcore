// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class NorthwindQuerySqliteFixture : NorthwindQueryRelationalFixture, IDisposable
    {
        private readonly DbContextOptions _options;

        private readonly SqliteTestStore _testStore = SqliteTestStore.GetNorthwindStore();

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public NorthwindQuerySqliteFixture()
        {
            _options = BuildOptions();
        }

        public override DbContextOptions BuildOptions(IServiceCollection additionalServices = null)
            => ConfigureOptions(
                    new DbContextOptionsBuilder()
                        .UseInternalServiceProvider(
                            (additionalServices ?? new ServiceCollection())
                            .AddEntityFrameworkSqlite()
                            .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                            .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                            .BuildServiceProvider()))
                .UseSqlite(
                    _testStore.ConnectionString,
                    b => ConfigureOptions(b).SuppressForeignKeyEnforcement())
                .Options;

        protected virtual DbContextOptionsBuilder ConfigureOptions(DbContextOptionsBuilder dbContextOptionsBuilder)
            => dbContextOptionsBuilder;

        protected virtual SqliteDbContextOptionsBuilder ConfigureOptions(SqliteDbContextOptionsBuilder sqliteDbContextOptionsBuilder)
            => sqliteDbContextOptionsBuilder;

        public override NorthwindContext CreateContext(
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
            => new NorthwindContext(_options, queryTrackingBehavior);

        public void Dispose() => _testStore.Dispose();
    }
}
