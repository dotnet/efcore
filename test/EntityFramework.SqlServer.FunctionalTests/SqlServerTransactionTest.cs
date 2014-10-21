// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerTransactionTest : TransactionTestBase<SqlServerTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public SqlServerTransactionTest()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .BuildServiceProvider();
        }

        protected override bool SnapshotSupported
        {
            get { return true; }
        }

        protected override async Task<SqlServerTestStore> CreateTestDatabaseAsync()
        {
            var db = await SqlServerTestStore.CreateScratchAsync();

            using (var command = db.Connection.CreateCommand())
            {
                command.CommandText = "ALTER DATABASE [" + db.Connection.Database + "] SET ALLOW_SNAPSHOT_ISOLATION ON";
                await command.ExecuteNonQueryAsync();
            }

            using (var command = db.Connection.CreateCommand())
            {
                command.CommandText = "ALTER DATABASE [" + db.Connection.Database + "] SET READ_COMMITTED_SNAPSHOT ON";
                await command.ExecuteNonQueryAsync();
            }

            using (var context = await CreateContextAsync(db))
            {
                await SeedAsync(context);
            }

            return db;
        }

        protected override Task<DbContext> CreateContextAsync(SqlServerTestStore testStore)
        {
            var options
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseSqlServer(testStore.Connection.ConnectionString);

            return Task.FromResult(new DbContext(_serviceProvider, options));
        }

        protected override Task<DbContext> CreateContextAsync(DbConnection connection)
        {
            var options
                = new DbContextOptions()
                    .UseModel(CreateModel())
                    .UseSqlServer(connection);

            return Task.FromResult(new DbContext(_serviceProvider, options));
        }
    }
}
