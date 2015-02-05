// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public abstract class SqlServerGraphUpdatesTestBase<TFixture> : GraphUpdatesTestBase<SqlServerTestStore, TFixture>
        where TFixture : SqlServerGraphUpdatesTestBase<TFixture>.SqlServerGraphUpdatesFixtureBase, new()
    {
        protected SqlServerGraphUpdatesTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public abstract class SqlServerGraphUpdatesFixtureBase : GraphUpdatesFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            protected SqlServerGraphUpdatesFixtureBase()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlServer()
                    .ServiceCollection
                    .AddSingleton<SqlServerModelSource>(p => new TestSqlServerModelSource(OnModelCreating))
                    .BuildServiceProvider();
            }

            protected abstract string DatabaseName { get; }

            public override SqlServerTestStore CreateTestStore()
            {
                return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var options = new DbContextOptions();
                        options.UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName));

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
        }
    }
}
