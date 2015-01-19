// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.GearsOfWarModel;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerGearsOfWarQueryFixture : RelationalGearsOfWarQueryFixture<SqlServerTestStore>
    {
        public static readonly string DatabaseName = "GearsOfWarQueryTest";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = SqlServerTestStore.CreateConnectionString(DatabaseName);

        public SqlServerGearsOfWarQueryFixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .AddTestModelSource(OnModelCreating)
                .AddInstance<ILoggerFactory>(new TestSqlLoggerFactory())
                .BuildServiceProvider();
        }

        public override SqlServerTestStore CreateTestStore()
        {
            return SqlServerTestStore.GetOrCreateSharedAsync(DatabaseName, async () =>
                {
                    var options = new DbContextOptions();
                    options.UseSqlServer(_connectionString);

                    using (var context = new GearsOfWarContext(_serviceProvider, options))
                    {
                        // TODO: Only delete if model changed
                        await context.Database.EnsureDeletedAsync();
                        if (await context.Database.EnsureCreatedAsync())
                        {
                            await GearsOfWarModelInitializer.SeedAsync(context);
                        }
                    }
                }).Result;
        }

        public override GearsOfWarContext CreateContext(SqlServerTestStore testStore)
        {
            var options = new DbContextOptions();
            options.UseSqlServer(testStore.Connection);

            var context = new GearsOfWarContext(_serviceProvider, options);
            context.Database.AsRelational().Connection.UseTransaction(testStore.Transaction);
            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ForSqlServer().UseSequence();
        }
    }
}

