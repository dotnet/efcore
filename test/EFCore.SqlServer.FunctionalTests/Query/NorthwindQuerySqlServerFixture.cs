// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class NorthwindQuerySqlServerFixture : NorthwindQueryRelationalFixture, IDisposable
    {
        private readonly SqlServerTestStore _testStore = SqlServerTestStore.GetNorthwindStore();

        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public override DbContextOptions BuildOptions(IServiceCollection additionalServices = null)
            => ConfigureOptions(
                    new DbContextOptionsBuilder()
                        .EnableSensitiveDataLogging()
                        .UseInternalServiceProvider(
                            (additionalServices ?? new ServiceCollection())
                            .AddEntityFrameworkSqlServer()
                            .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                            .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                            .BuildServiceProvider(validateScopes: true)))
                .UseSqlServer(
                    _testStore.ConnectionString,
                    b =>
                        {
                            b.ApplyConfiguration();
                            ConfigureOptions(b);
                            b.ApplyConfiguration();
                        })
                .Options;

        protected virtual DbContextOptionsBuilder ConfigureOptions(DbContextOptionsBuilder dbContextOptionsBuilder)
            => dbContextOptionsBuilder;

        protected virtual void ConfigureOptions(SqlServerDbContextOptionsBuilder sqlServerDbContextOptionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>()
                .Property(c => c.CustomerID)
                .HasColumnType("nchar(5)");

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasColumnType("money");

            modelBuilder.Entity<Product>()
                .Property(p => p.UnitPrice)
                .HasColumnType("money");
        }

        public void Dispose() => _testStore.Dispose();
    }
}
