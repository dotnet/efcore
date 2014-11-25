// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Data.SQLite;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class SqLiteTransactionFixture : TransactionFixtureBase<SqLiteTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public SqLiteTransactionFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlite()
                .ServiceCollection
                .AddTestModelSource(OnModelCreating)
                .BuildServiceProvider();
        }

        public override SqLiteTestStore CreateTestStore()
        {
            var db = SqLiteTestStore.CreateScratchAsync().Result;
            using (var context = CreateContext(db))
            {
                Seed(context);
            }

            return db;
        }

        public override DbContext CreateContext(SqLiteTestStore testStore)
        {
            var sb = new SQLiteConnectionStringBuilder(testStore.Connection.ConnectionString);

            var options = new DbContextOptions()
                .UseSqlite(sb.ConnectionString);

            return new DbContext(_serviceProvider, options);
        }

        public override DbContext CreateContext(DbConnection connection)
        {
            var options = new DbContextOptions()
                .UseSqlite(connection);

            return new DbContext(_serviceProvider, options);
        }
    }
}
