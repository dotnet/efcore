// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerOptimisticConcurrencyTest : OptimisticConcurrencyRelationalTestBase<SqlServerTestDatabase>
    {
        private readonly IServiceProvider _serviceProvider;

        public SqlServerOptimisticConcurrencyTest()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .BuildServiceProvider();
        }

        protected override Task<SqlServerTestDatabase> CreateTestDatabaseAsync()
        {
            return SqlServerTestDatabase.Named(DatabaseName, async () =>
                {
                    using (var context = CreateF1Context(SqlServerTestDatabase.CreateConnectionString(DatabaseName)))
                    {
                        await ConcurrencyModelInitializer.SeedAsync(context);
                    }
                });
        }

        protected override DataStoreTransaction BeginTransaction(F1Context context, SqlServerTestDatabase testDatabase, Action<F1Context> prepareStore)
        {
            var transaction = context.Database.AsRelational().Connection.BeginTransaction();

            testDatabase.Transaction = (SqlTransaction)transaction.DbTransaction;

            using (var innerContext = CreateF1Context(testDatabase))
            {
                prepareStore(innerContext);
                innerContext.SaveChanges();
            }

            return transaction;
        }

        public F1Context CreateF1Context(string connectionString)
        {
            var options
                = new DbContextOptions()
                    .UseModel(AddStoreMetadata(F1Context.CreateModel()))
                    .UseSqlServer(connectionString);

            return new F1Context(_serviceProvider, options);
        }

        protected override F1Context CreateF1Context(SqlServerTestDatabase testDatabase)
        {
            var options
                = new DbContextOptions()
                    .UseModel(AddStoreMetadata(F1Context.CreateModel()))
                    .UseSqlServer(testDatabase.Connection);

            var context = new F1Context(_serviceProvider, options);
            context.Database.AsRelational().Connection.UseTransaction(testDatabase.Transaction);
            return context;
        }
    }
}
