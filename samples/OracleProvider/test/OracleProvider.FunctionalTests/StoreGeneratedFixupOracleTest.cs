// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class StoreGeneratedFixupOracleTest : StoreGeneratedFixupRelationalTestBase<StoreGeneratedFixupOracleTest.StoreGeneratedFixupOracleFixture>
    {
        public StoreGeneratedFixupOracleTest(StoreGeneratedFixupOracleFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        }

        [Fact]
        public void Temp_values_are_replaced_on_save()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                    {
                        var entry = context.Add(new TestTemp());

                        Assert.True(entry.Property(e => e.Id).IsTemporary);
                        Assert.False(entry.Property(e => e.NotId).IsTemporary);

                        var tempValue = entry.Property(e => e.Id).CurrentValue;

                        context.SaveChanges();

                        Assert.False(entry.Property(e => e.Id).IsTemporary);
                        Assert.NotEqual(tempValue, entry.Property(e => e.Id).CurrentValue);
                    });
        }

        protected override void MarkIdsTemporary(DbContext context, object dependent, object principal)
        {
            var entry = context.Entry(dependent);
            entry.Property("Id1").IsTemporary = true;
            entry.Property("Id2").IsTemporary = true;

            entry = context.Entry(principal);
            entry.Property("Id1").IsTemporary = true;
            entry.Property("Id2").IsTemporary = true;
        }

        protected override void MarkIdsTemporary(DbContext context, object game, object level, object item)
        {
            var entry = context.Entry(game);
            entry.Property("Id").IsTemporary = true;

            entry = context.Entry(item);
            entry.Property("Id").IsTemporary = true;
        }

        protected override bool EnforcesFKs => true;

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class StoreGeneratedFixupOracleFixture : StoreGeneratedFixupRelationalFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;

            public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<Parent>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<Child>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<ParentPN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<ChildPN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<ParentDN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<ChildDN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<ParentNN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<ChildNN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<CategoryDN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<ProductDN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<CategoryPN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<ProductPN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<CategoryNN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<ProductNN>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<Category>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<Product>(
                    b =>
                        {
                            b.Property(e => e.Id1).ValueGeneratedOnAdd();
                            b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()");
                        });

                modelBuilder.Entity<Item>(b => b.Property(e => e.Id).ValueGeneratedOnAdd());

                modelBuilder.Entity<Game>(b => b.Property(e => e.Id).ValueGeneratedOnAdd().HasDefaultValueSql("SYS_GUID()"));
            }
        }
    }
}
