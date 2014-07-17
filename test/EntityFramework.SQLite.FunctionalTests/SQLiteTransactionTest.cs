// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Data.SQLite;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SQLite.FunctionalTests
{
    public class SqlServerTransactionTest : TransactionTestBase<SQLiteTestDatabase>
    {
        private readonly IServiceProvider _serviceProvider;

        public SqlServerTransactionTest()
        {
            _serviceProvider
                = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSQLite()
                    .ServiceCollection
                    .BuildServiceProvider();
        }

        protected override bool SnapshotSupported
        {
            get { return false; }
        }

        protected override async Task<SQLiteTestDatabase> CreateTestDatabaseAsync()
        {
            var db = await SQLiteTestDatabase.Scratch();
            using (var context = await CreateContextAsync(db))
            {
                await SeedAsync(context);
            }

            return db;
        }

        protected override Task<DbContext> CreateContextAsync(SQLiteTestDatabase testDatabase)
        {
            var sb = new SQLiteConnectionStringBuilder(testDatabase.Connection.ConnectionString);

            var options
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseSQLite(sb.ConnectionString);

            return Task.FromResult(new DbContext(_serviceProvider, options));
        }

        protected override Task<DbContext> CreateContextAsync(DbConnection connection)
        {
            var options
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseSQLite(connection);

            return Task.FromResult(new DbContext(_serviceProvider, options));
        }
    }
}
