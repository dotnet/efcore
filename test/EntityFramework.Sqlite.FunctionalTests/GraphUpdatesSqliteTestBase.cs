// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public abstract class GraphUpdatesSqliteTestBase<TFixture> : GraphUpdatesTestBase<SqliteTestStore, TFixture>
        where TFixture : GraphUpdatesSqliteTestBase<TFixture>.GraphUpdatesSqliteFixtureBase, new()
    {
        protected GraphUpdatesSqliteTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public override void Required_many_to_one_dependents_are_cascade_deleted()
        {
            // Cascade delete not yet supported by SQLite provider
        }

        public abstract class GraphUpdatesSqliteFixtureBase : GraphUpdatesFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            protected GraphUpdatesSqliteFixtureBase()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlite()
                    .ServiceCollection()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            protected abstract string DatabaseName { get; }

            public override SqliteTestStore CreateTestStore()
            {
                return SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder();
                        optionsBuilder.UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName));

                        using (var context = new GraphUpdatesContext(_serviceProvider, optionsBuilder.Options))
                        {
                            context.Database.EnsureDeleted();
                            if (context.Database.EnsureCreated())
                            {
                                Seed(context);
                            }
                        }
                    });
            }

            public override DbContext CreateContext(SqliteTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlite(testStore.Connection);

                var context = new GraphUpdatesContext(_serviceProvider, optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);
                return context;
            }
        }
    }
}
