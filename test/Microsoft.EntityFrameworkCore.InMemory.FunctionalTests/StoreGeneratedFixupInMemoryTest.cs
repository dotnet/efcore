// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore.InMemory.FunctionalTests
{
    public class StoreGeneratedFixupInMemoryTest
        : StoreGeneratedFixupTestBase<InMemoryTestStore, StoreGeneratedFixupInMemoryTest.StoreGeneratedFixupInMemoryFixture>
    {
        public StoreGeneratedFixupInMemoryTest(StoreGeneratedFixupInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public void InMemory_database_does_not_use_temp_values()
        {
            using (var context = CreateContext())
            {
                var entry = context.Add(new TestTemp());

                Assert.False(entry.Property(e => e.Id).IsTemporary);
                Assert.False(entry.Property(e => e.NotId).IsTemporary);

                var tempValue = entry.Property(e => e.Id).CurrentValue;

                context.SaveChanges();

                Assert.False(entry.Property(e => e.Id).IsTemporary);
                Assert.Equal(tempValue, entry.Property(e => e.Id).CurrentValue);
            }
        }

        protected override void MarkIdsTemporary(StoreGeneratedFixupContext context, object dependent, object principal)
        {
        }

        protected override void MarkIdsTemporary(StoreGeneratedFixupContext context, object game, object level, object item)
        {
        }

        protected override bool EnforcesFKs => false;

        public class StoreGeneratedFixupInMemoryFixture : StoreGeneratedFixupFixtureBase
        {
            private readonly IServiceProvider _serviceProvider;

            public StoreGeneratedFixupInMemoryFixture()
            {
                _serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddSingleton(TestInMemoryModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();
            }

            public override InMemoryTestStore CreateTestStore()
            {
                var store = new InMemoryStoreGeneratedFixupTestStore(_serviceProvider);

                using (var context = CreateContext(store))
                {
                    Seed(context);
                }

                return store;
            }

            public override DbContext CreateContext(InMemoryTestStore testStore)
                => new StoreGeneratedFixupContext(new DbContextOptionsBuilder()
                    .UseInMemoryDatabase()
                    .UseInternalServiceProvider(_serviceProvider).Options);

            public class InMemoryStoreGeneratedFixupTestStore : InMemoryTestStore
            {
                private readonly IServiceProvider _serviceProvider;

                public InMemoryStoreGeneratedFixupTestStore(IServiceProvider serviceProvider)
                {
                    _serviceProvider = serviceProvider;
                }

                public override void Dispose()
                {
                    _serviceProvider.GetRequiredService<IInMemoryStoreSource>().GetGlobalStore().Clear();

                    base.Dispose();
                }
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<Parent>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<Child>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<ParentPN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<ChildPN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<ParentDN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<ChildDN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<ParentNN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<ChildNN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<CategoryDN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<ProductDN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<CategoryPN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<ProductPN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<CategoryNN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<ProductNN>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<Category>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<Product>(b =>
                    {
                        b.Property(e => e.Id1).ValueGeneratedOnAdd();
                        b.Property(e => e.Id2).ValueGeneratedOnAdd();
                    });

                modelBuilder.Entity<Item>(b => { b.Property(e => e.Id).ValueGeneratedOnAdd(); });

                modelBuilder.Entity<Game>(b => { b.Property(e => e.Id).ValueGeneratedOnAdd(); });
            }
        }
    }
}
