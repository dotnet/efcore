// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public class TableSplittingSqlServerTest : TableSplittingTestBase<SqlServerTestStore>
    {
        private readonly string _connectionString = SqlServerTestStore.CreateConnectionString(DatabaseName);
        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public override SqlServerTestStore CreateTestStore(Action<ModelBuilder> onModelCreating)
            => SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseSqlServer(_connectionString, b => b.ApplyConfiguration().CommandTimeout(300))
                        .EnableSensitiveDataLogging()
                        .UseInternalServiceProvider(BuildServiceProvider(onModelCreating));

                    using (var context = new TransportationContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureCreated();
                        context.Seed();
                    }
                });

        public override TransportationContext CreateContext(SqlServerTestStore testStore, Action<ModelBuilder> onModelCreating)
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseSqlServer(testStore.Connection, b => b.ApplyConfiguration().CommandTimeout(300))
                .EnableSensitiveDataLogging()
                .UseInternalServiceProvider(BuildServiceProvider(onModelCreating));

            var context = new TransportationContext(optionsBuilder.Options);
            context.Database.UseTransaction(testStore.Transaction);
            return context;
        }

        private IServiceProvider BuildServiceProvider(Action<ModelBuilder> onModelCreating)
            => new ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddSingleton(TestModelSource.GetFactory(onModelCreating))
                .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                .BuildServiceProvider();
    }
}
