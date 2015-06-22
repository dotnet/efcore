// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class StoreGeneratedSqliteTest
        : StoreGeneratedTestBase<SqliteTestStore, StoreGeneratedSqliteTest.StoreGeneratedSqliteFixture>
    {
        public StoreGeneratedSqliteTest(StoreGeneratedSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class StoreGeneratedSqliteFixture : StoreGeneratedFixtureBase
        {
            private const string DatabaseName = "StoreGeneratedTest";

            private readonly IServiceProvider _serviceProvider;

            public StoreGeneratedSqliteFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFramework()
                    .AddSqlite()
                    .ServiceCollection()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override SqliteTestStore CreateTestStore()
            {
                return SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder();
                        optionsBuilder.UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName));

                        using (var context = new StoreGeneratedContext(_serviceProvider, optionsBuilder.Options))
                        {
                            context.Database.EnsureDeleted();
                            context.Database.EnsureCreated();
                        }
                    });
            }

            public override DbContext CreateContext(SqliteTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder();
                optionsBuilder.UseSqlite(testStore.Connection);

                var context = new StoreGeneratedContext(_serviceProvider, optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);

                return context;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Gumball>(b =>
                    {
                        b.Property(e => e.Identity)
                            .DefaultValue("Banana Joe");

                        b.Property(e => e.IdentityReadOnlyBeforeSave)
                            .DefaultValue("Doughnut Sheriff");

                        b.Property(e => e.IdentityReadOnlyAfterSave)
                            .DefaultValue("Anton");

                        b.Property(e => e.Computed)
                            .DefaultValue("Alan");

                        b.Property(e => e.ComputedReadOnlyBeforeSave)
                            .DefaultValue("Carmen");

                        b.Property(e => e.ComputedReadOnlyAfterSave)
                            .DefaultValue("Tina Rex");
                    });
            }
        }
    }
}
