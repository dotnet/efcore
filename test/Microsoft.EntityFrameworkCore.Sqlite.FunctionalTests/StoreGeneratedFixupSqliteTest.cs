// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Sqlite.FunctionalTests
{
    public class StoreGeneratedFixupSqliteTest
        : StoreGeneratedFixupTestBase<SqliteTestStore, StoreGeneratedFixupSqliteTest.StoreGeneratedFixupSqliteFixture>
    {
        public StoreGeneratedFixupSqliteTest(StoreGeneratedFixupSqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override void MarkIdsTemporary(StoreGeneratedFixupContext context, object dependent, object principal)
        {
            // TODO: Uncomment this when #6292 is fixed
            //var entry = context.Entry(dependent);
            //entry.GetInfrastructure().MarkAsTemporary(entry.Property("Id1").Metadata);

            //entry = context.Entry(principal);
            //entry.GetInfrastructure().MarkAsTemporary(entry.Property("Id1").Metadata);
        }

        protected override void MarkIdsTemporary(StoreGeneratedFixupContext context, object game, object level, object item)
        {
            // TODO: Uncomment this when #6292 is fixed
            //var entry = context.Entry(game);
            //entry.GetInfrastructure().MarkAsTemporary(entry.Property("Id").Metadata);

            //entry = context.Entry(item);
            //entry.GetInfrastructure().MarkAsTemporary(entry.Property("Id").Metadata);
        }

        protected override bool EnforcesFKs => true;

        public class StoreGeneratedFixupSqliteFixture : StoreGeneratedFixupFixtureBase
        {
            private const string DatabaseName = "StoreGeneratedFixup";

            private readonly IServiceProvider _serviceProvider;

            public StoreGeneratedFixupSqliteFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .AddSingleton(TestSqliteModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override SqliteTestStore CreateTestStore()
            {
                return SqliteTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder()
                            .UseSqlite(SqliteTestStore.CreateConnectionString(DatabaseName))
                            .UseInternalServiceProvider(_serviceProvider);

                        using (var context = new StoreGeneratedFixupContext(optionsBuilder.Options))
                        {
                            context.Database.EnsureClean();
                            Seed(context);
                        }
                    });
            }

            public override DbContext CreateContext(SqliteTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseSqlite(testStore.Connection)
                    .UseInternalServiceProvider(_serviceProvider);

                var context = new StoreGeneratedFixupContext(optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);

                return context;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Parent>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<Child>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<ParentPN>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<ChildPN>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<ParentDN>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<ChildDN>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<ParentNN>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<ChildNN>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<CategoryDN>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<ProductDN>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<CategoryPN>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<ProductPN>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<CategoryNN>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<ProductNN>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<Category>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<Product>(b => { b.Property(e => e.Id1).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<Item>(b => { b.Property(e => e.Id).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<Game>(b => { b.Property(e => e.Id).ValueGeneratedOnAdd(); });
            }
        }
    }
}
