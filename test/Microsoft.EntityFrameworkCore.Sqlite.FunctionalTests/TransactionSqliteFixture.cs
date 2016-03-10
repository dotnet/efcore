// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.FunctionalTests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class TransactionSqliteFixture : TransactionFixtureBase<SqliteTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionSqliteFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();
        }

        public override SqliteTestStore CreateTestStore()
        {
            var db = SqliteTestStore.CreateScratch(sharedCache: true);

            using (var context = CreateContext(db))
            {
                Seed(context);
            }

            return db;
        }

        public override DbContext CreateContext(SqliteTestStore testStore)
            => new DbContext(new DbContextOptionsBuilder()
                .UseSqlite(testStore.ConnectionString)
                .UseInternalServiceProvider(_serviceProvider).Options);

        public override DbContext CreateContext(DbConnection connection)
            => new DbContext(new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .UseInternalServiceProvider(_serviceProvider).Options);
    }
}
