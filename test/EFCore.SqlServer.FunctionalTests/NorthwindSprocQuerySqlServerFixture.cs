// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class NorthwindSprocQuerySqlServerFixture : NorthwindSprocQueryRelationalFixture, IDisposable
    {
        private readonly DbContextOptions _options;
        private readonly SqlServerTestStore _testStore;

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public NorthwindSprocQuerySqlServerFixture()
        {
            _testStore = SqlServerTestStore.GetNorthwindStore();

            _options = BuildOptions();
        }

        public override DbContextOptions BuildOptions(IServiceCollection additionalServices = null)
            => new DbContextOptionsBuilder()
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider((additionalServices ?? new ServiceCollection())
                    .AddEntityFrameworkSqlServer()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                    .BuildServiceProvider())
                .UseSqlServer(_testStore.ConnectionString, b => b.ApplyConfiguration()).Options;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice).ForSqlServerHasColumnType("money");
        }

        public override NorthwindContext CreateContext(
            QueryTrackingBehavior queryTrackingBehavior = QueryTrackingBehavior.TrackAll)
        {
            var context = new NorthwindContext(_options, queryTrackingBehavior);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }

        public void Dispose() => _testStore.Dispose();
    }
}
