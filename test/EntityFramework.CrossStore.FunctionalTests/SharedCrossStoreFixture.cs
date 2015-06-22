// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Data.Entity.InMemory.FunctionalTests;
using Microsoft.Data.Entity.Sqlite.FunctionalTests;
using Microsoft.Data.Entity.SqlServer.FunctionalTests;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class SharedCrossStoreFixture : CrossStoreFixture
    {
        private readonly IServiceProvider _serviceProvider;
        private Guid id = Guid.NewGuid();

        public SharedCrossStoreFixture()
            : this(new ServiceCollection()
                .AddEntityFramework()
                .AddInMemoryDatabase()
                .AddSqlite()
                .AddSqlServer()
                .ServiceCollection()
                .BuildServiceProvider())
        {
        }

        public SharedCrossStoreFixture(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override TestStore CreateTestStore(Type testStoreType)
        {
            if (testStoreType == typeof(InMemoryTestStore))
            {
                return new InMemoryTestStore();
            }

            if (testStoreType == typeof(SqlServerTestStore))
            {
                return SqlServerTestStore.CreateScratch();
            }

            if (testStoreType == typeof(SqliteTestStore))
            {
                return SqliteTestStore.CreateScratch();
            }

            throw new NotImplementedException();
        }

        public override CrossStoreContext CreateContext(TestStore testStore)
        {
            var inMemoryTestStore = testStore as InMemoryTestStore;
            if (inMemoryTestStore != null)
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseInMemoryDatabase();

                return new CrossStoreContext(_serviceProvider, optionsBuilder.Options);
            }

            var sqliteTestStore = testStore as SqliteTestStore;
            if (sqliteTestStore != null)
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlite(sqliteTestStore.Connection);

                var context = new CrossStoreContext(_serviceProvider, optionsBuilder.Options);
                context.Database.EnsureCreated();
                context.Database.UseTransaction(sqliteTestStore.Transaction);

                return context;
            }

            var sqlServerTestStore = testStore as SqlServerTestStore;
            if (sqlServerTestStore != null)
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlServer(sqlServerTestStore.Connection);

                var context = new CrossStoreContext(_serviceProvider, optionsBuilder.Options);
                context.Database.EnsureCreated();
                context.Database.UseTransaction(sqlServerTestStore.Transaction);

                return context;
            }

            throw new NotImplementedException();
        }
    }
}
