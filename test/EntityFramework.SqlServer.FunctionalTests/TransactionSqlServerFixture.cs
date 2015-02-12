// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class TransactionSqlServerFixture : TransactionFixtureBase<SqlServerTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionSqlServerFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection()
                .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();
        }

        public override SqlServerTestStore CreateTestStore()
        {
            var db = SqlServerTestStore.CreateScratch();

            using (var command = db.Connection.CreateCommand())
            {
                command.CommandText = "ALTER DATABASE [" + db.Connection.Database + "] SET ALLOW_SNAPSHOT_ISOLATION ON";
                command.ExecuteNonQuery();
            }

            using (var command = db.Connection.CreateCommand())
            {
                command.CommandText = "ALTER DATABASE [" + db.Connection.Database + "] SET READ_COMMITTED_SNAPSHOT ON";
                command.ExecuteNonQuery();
            }

            using (var context = CreateContext(db))
            {
                Seed(context);
            }

            return db;
        }

        public override DbContext CreateContext(SqlServerTestStore testStore)
        {
            var options = new DbContextOptions();
            options.UseSqlServer(testStore.Connection.ConnectionString);

            return new DbContext(_serviceProvider, options);
        }

        public override DbContext CreateContext(DbConnection connection)
        {
            var options = new DbContextOptions();
            options.UseSqlServer(connection);

            return new DbContext(_serviceProvider, options);
        }
    }
}
