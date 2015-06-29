// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class TransactionSqliteFixture : TransactionFixtureBase<SqliteTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionSqliteFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection()
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
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite(testStore.Connection.ConnectionString);

            return new DbContext(_serviceProvider, optionsBuilder.Options);
        }

        public override DbContext CreateContext(DbConnection connection)
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseSqlite(connection);

            return new DbContext(_serviceProvider, optionsBuilder.Options);
        }
    }
}
