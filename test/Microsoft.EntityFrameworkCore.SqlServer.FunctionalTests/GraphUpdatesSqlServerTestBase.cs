// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public abstract class GraphUpdatesSqlServerTestBase<TFixture> : GraphUpdatesTestBase<SqlServerTestStore, TFixture>
        where TFixture : GraphUpdatesSqlServerTestBase<TFixture>.GraphUpdatesSqlServerFixtureBase, new()
    {
        protected GraphUpdatesSqlServerTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public abstract class GraphUpdatesSqlServerFixtureBase : GraphUpdatesFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            protected GraphUpdatesSqlServerFixtureBase()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            protected abstract string DatabaseName { get; }

            public override SqlServerTestStore CreateTestStore()
            {
                return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder()
                            .UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName))
                            .UseInternalServiceProvider(_serviceProvider);

                        using (var context = new GraphUpdatesContext(optionsBuilder.Options))
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
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseSqlServer(testStore.Connection)
                    .UseInternalServiceProvider(_serviceProvider);

                var context = new GraphUpdatesContext(optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);
                return context;
            }
        }
    }
}
