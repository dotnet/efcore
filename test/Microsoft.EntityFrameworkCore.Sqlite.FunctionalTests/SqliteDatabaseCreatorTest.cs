// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class SqliteDatabaseCreatorTest
    {
        [Fact]
        public void Exists_returns_false_when_database_doesnt_exist()
        {
            var creator = GetDatabaseCreator("Data Source=doesnt-exist.db");

            Assert.False(creator.Exists());
        }

        [Fact]
        public void Exists_returns_true_when_database_exists()
        {
            using (var testStore = SqliteTestStore.CreateScratch())
            {
                var creator = GetDatabaseCreator(testStore.ConnectionString);

                Assert.True(creator.Exists());
            }
        }

        private IRelationalDatabaseCreator GetDatabaseCreator(string connectionString)
            => new DbContext(new DbContextOptionsBuilder()
                    .UseSqlite(connectionString)
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkSqlite()
                        .BuildServiceProvider())
                    .Options)
                .GetService<IRelationalDatabaseCreator>();
    }
}
