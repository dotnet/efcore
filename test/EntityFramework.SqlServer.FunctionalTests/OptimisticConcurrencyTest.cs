// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using ConcurrencyModel;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class OptimisticConcurrencyTest : OptimisticConcurrencyRelationalTestBase<SqlServerTestDatabase>
    {
        private readonly IServiceProvider _serviceProvider;

        public OptimisticConcurrencyTest()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .BuildServiceProvider();
        }

        protected override async Task<SqlServerTestDatabase> CreateTestDatabaseAsync()
        {
            var db = await SqlServerTestDatabase.Scratch();

            using (var context = await CreateF1ContextAsync(db))
            {
                await ConcurrencyModelInitializer.SeedAsync(context);
            }

            return db;
        }

        protected override Task<F1Context> CreateF1ContextAsync(SqlServerTestDatabase testDatabase)
        {
            var options
                = new DbContextOptions()
                    .UseModel(AddStoreMetadata(F1Context.CreateModel()))
                    .UseSqlServer(testDatabase.Connection.ConnectionString);

            return Task.FromResult(new F1Context(_serviceProvider, options));
        }
    }
}
