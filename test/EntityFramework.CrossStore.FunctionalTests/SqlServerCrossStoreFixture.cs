// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Data.Entity.SqlServer.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public class SqlServerCrossStoreFixture : CrossStoreFixture<SqlServerTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public SqlServerCrossStoreFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .AddInMemoryStore()
                .ServiceCollection
                .BuildServiceProvider();
        }

        public override SqlServerTestStore CreateTestStore()
        {
            return SqlServerTestStore.CreateScratchAsync().Result;
        }

        public override CrossStoreContext CreateContext(SqlServerTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseSqlServer(testStore.Connection);

            var context = new CrossStoreContext(_serviceProvider, options);
            context.Database.EnsureCreated();
            context.Database.AsRelational().Connection.UseTransaction(testStore.Transaction);

            return context;
        }
    }
}
