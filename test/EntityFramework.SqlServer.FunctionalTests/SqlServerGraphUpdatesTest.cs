// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerGraphUpdatesTest : GraphUpdatesTestBase<SqlServerTestStore, SqlServerGraphUpdatesTest.SqlServerGraphUpdatesFixture>
    {
        public SqlServerGraphUpdatesTest(SqlServerGraphUpdatesFixture fixture)
            : base(fixture)
        {
        }

        public class SqlServerGraphUpdatesFixture : GraphUpdatesFixtureBase
        {
            public static readonly string DatabaseName = "GraphUpdatesTest";

            private readonly IServiceProvider _serviceProvider;

            private readonly string _connectionString = SqlServerTestStore.CreateConnectionString(DatabaseName);

            public SqlServerGraphUpdatesFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection
                    .AddSingleton<SqlServerModelSource>(p => new TestSqlServerModelSource(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override SqlServerTestStore CreateTestStore()
            {
                return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var options = new DbContextOptions();
                        options.UseSqlServer(_connectionString);

                        using (var context = new GraphUpdatesContext(_serviceProvider, options))
                        {
                            context.Database.EnsureDeleted();
                            if (context.Database.EnsureCreated())
                            {
                                Seed(context);
                            }
                        }
                    });
            }

            public override DbContext CreateContext(SqlServerTestStore testStore)
            {
                var options = new DbContextOptions();
                options.UseSqlServer(testStore.Connection);

                var context = new GraphUpdatesContext(_serviceProvider, options);
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
}
