// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
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
            using (var testStore = SqliteTestStore.GetOrCreateInitialized("Empty"))
            {
                var creator = GetDatabaseCreator(testStore.ConnectionString);

                Assert.True(creator.Exists());
            }
        }

        private IRelationalDatabaseCreator GetDatabaseCreator(string connectionString)
            => new DbContext(new DbContextOptionsBuilder()
                .UseSqlite(connectionString)
                .UseInternalServiceProvider(SqliteTestStoreFactory.Instance.AddProviderServices(new ServiceCollection())
                    .BuildServiceProvider(validateScopes: true))
                .Options)
                .GetService<IRelationalDatabaseCreator>();
    }
}
