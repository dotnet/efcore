// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerBuiltInDataTypesFixture : BuiltInDataTypesFixtureBase<SqlServerTestStore>
    {
        private readonly IServiceProvider _serviceProvider;

        public SqlServerBuiltInDataTypesFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .BuildServiceProvider();
        }

        public override SqlServerTestStore CreateTestStore()
        {
            return SqlServerTestStore.CreateScratchAsync().Result;
        }

        public override DbContext CreateContext(SqlServerTestStore testStore)
        {
            var options = new DbContextOptions()
                .UseModel(CreateModel())
                .UseSqlServer(testStore.Connection);

            var context = new DbContext(_serviceProvider, options);
            context.Database.EnsureCreated();
            context.Database.AsRelational().Connection.UseTransaction(testStore.Transaction);
            return context;
        }
    }
}
