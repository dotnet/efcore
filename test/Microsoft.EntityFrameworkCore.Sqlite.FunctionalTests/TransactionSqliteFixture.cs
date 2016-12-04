// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class TransactionSqliteFixture : TransactionFixtureBase<SqliteTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public TransactionSqliteFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                .BuildServiceProvider();
        }

        public override SqliteTestStore CreateTestStore()
        {
            return SqliteTestStore.GetOrCreateShared(DatabaseName, false, true, () =>
                {
                    var optionsBuilder = new DbContextOptionsBuilder()
                        .UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName))
                        .UseInternalServiceProvider(_serviceProvider);

                    using (var context = new DbContext(optionsBuilder.Options))
                    {
                        context.Database.EnsureClean();
                    }
                });
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
