// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class StoreGeneratedFixupSqlServerTest
        : StoreGeneratedFixupTestBase<SqlServerTestStore, StoreGeneratedFixupSqlServerTest.StoreGeneratedFixupSqlServerFixture>
    {
        public StoreGeneratedFixupSqlServerTest(StoreGeneratedFixupSqlServerFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void Temp_values_are_replaced_on_save()
        {
            using (var context = CreateContext())
            {
                var entry = context.Add(new TestTemp());

                Assert.True(entry.Property(e => e.Id).IsTemporary);
                Assert.False(entry.Property(e => e.NotId).IsTemporary);

                var tempValue = entry.Property(e => e.Id).CurrentValue;

                context.SaveChanges();

                Assert.False(entry.Property(e => e.Id).IsTemporary);
                Assert.NotEqual(tempValue, entry.Property(e => e.Id).CurrentValue);
            }
        }

        protected override void MarkIdsTemporary(StoreGeneratedFixupContext context, object dependent, object principal)
        {
            var entry = context.Entry(dependent);
            entry.Property("Id1").IsTemporary = true;
            entry.Property("Id2").IsTemporary = true;

            entry = context.Entry(principal);
            entry.Property("Id1").IsTemporary = true;
            entry.Property("Id2").IsTemporary = true;
        }

        protected override void MarkIdsTemporary(StoreGeneratedFixupContext context, object game, object level, object item)
        {
            var entry = context.Entry(game);
            entry.Property("Id").IsTemporary = true;

            entry = context.Entry(item);
            entry.Property("Id").IsTemporary = true;
        }

        protected override bool EnforcesFKs => true;

        public class StoreGeneratedFixupSqlServerFixture : StoreGeneratedFixupFixtureBase
        {
            private const string DatabaseName = "StoreGeneratedFixup";

            private readonly IServiceProvider _serviceProvider;

            public StoreGeneratedFixupSqlServerFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlServer()
                    .AddSingleton(TestSqlServerModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override SqlServerTestStore CreateTestStore()
            {
                return SqlServerTestStore.GetOrCreateShared(DatabaseName, () =>
                    {
                        var optionsBuilder = new DbContextOptionsBuilder()
                            .UseSqlServer(SqlServerTestStore.CreateConnectionString(DatabaseName))
                            .UseInternalServiceProvider(_serviceProvider);

                        using (var context = new StoreGeneratedFixupContext(optionsBuilder.Options))
                        {
                            context.Database.EnsureClean();
                            Seed(context);
                        }
                    });
            }

            public override DbContext CreateContext(SqlServerTestStore testStore)
            {
                var optionsBuilder = new DbContextOptionsBuilder()
                    .UseSqlServer(testStore.Connection)
                    .UseInternalServiceProvider(_serviceProvider);

                var context = new StoreGeneratedFixupContext(optionsBuilder.Options);
                context.Database.UseTransaction(testStore.Transaction);

                return context;
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Parent>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<Child>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<ParentPN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<ChildPN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<ParentDN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<ChildDN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<ParentNN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<ChildNN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<CategoryDN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<ProductDN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<CategoryPN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<ProductPN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<CategoryNN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<ProductNN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<Category>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<Product>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                    });

                modelBuilder.Entity<Item>(b => { b.Property(e => e.Id).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<Game>(b => { b.Property(e => e.Id).ValueGeneratedOnAdd().HasDefaultValueSql("newid()"); });
            }
        }
    }
}
