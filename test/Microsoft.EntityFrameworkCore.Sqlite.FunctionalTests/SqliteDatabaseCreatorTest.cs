// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
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
        {
            var services = new ServiceCollection();
            services
                .AddEntityFramework()
                .AddSqlite();

            var options = new DbContextOptionsBuilder();
            options.UseSqlite(connectionString);

            return new DbContext(services.BuildServiceProvider(), options.Options)
                .GetService<IRelationalDatabaseCreator>();
        }
    }
}
