// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;

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
                .UseLoggerFactory<LoggerFactory>()
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

        public F1Context CreateF1Context(string connectionString)
        {
            var modelBuilder = new ModelBuilder(new Model());
            AddStoreMetadata(modelBuilder);
            modelBuilder.ForSqlServer().UseSequence();

            var options
                = new DbContextOptions()
                    .UseModel(F1Context.CreateModel(modelBuilder).Model)
                    .UseSqlServer(connectionString);

            return new F1Context(_serviceProvider, options);
        }

        protected override F1Context CreateF1Context(SqlServerTestDatabase testDatabase)
        {
            var modelBuilder = new ModelBuilder(new Model());
            AddStoreMetadata(modelBuilder);
            modelBuilder.ForSqlServer().UseSequence();

            var options
                = new DbContextOptions()
                    .UseModel(F1Context.CreateModel(modelBuilder).Model)
                    .UseSqlServer(testDatabase.Connection);

            var context = new F1Context(_serviceProvider, options);
            context.Database.AsRelational().Connection.UseTransaction(testDatabase.Transaction);
            return context;
        }
    }
}
