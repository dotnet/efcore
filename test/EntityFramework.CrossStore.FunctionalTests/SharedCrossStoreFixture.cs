// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Data.Entity.InMemory.FunctionalTests;
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
                .AddInMemoryStore()
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
                return SqlServerTestStore.CreateScratchAsync().Result;
            }

            throw new NotImplementedException();
        }

        public override CrossStoreContext CreateContext(TestStore testStore)
        {
            var inMemoryTestStore = testStore as InMemoryTestStore;
            if (inMemoryTestStore != null)
            {
                var options = new DbContextOptions();
                options.UseInMemoryStore();

                return new CrossStoreContext(_serviceProvider, options);
            }

            var sqlServerTestStore = testStore as SqlServerTestStore;
            if (sqlServerTestStore != null)
            {
                var options = new DbContextOptions();
                options.UseSqlServer(sqlServerTestStore.Connection);

                var context = new CrossStoreContext(_serviceProvider, options);
                context.Database.EnsureCreated();
                context.Database.AsRelational().Connection.UseTransaction(sqlServerTestStore.Transaction);

                return context;
            }

            throw new NotImplementedException();
        }
    }
}
