// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    [SqlServerCondition(SqlServerCondition.SupportsMemoryOptimized)]
    public class MemoryOptimizedTablesTest : IClassFixture<MemoryOptimizedTablesTest.MemoryOptimizedTablesFixture>
    {
        protected MemoryOptimizedTablesFixture Fixture { get; }

        public MemoryOptimizedTablesTest(MemoryOptimizedTablesFixture fixture) => Fixture = fixture;

        [ConditionalFact]
        public void Can_create_memoryOptimized_table()
        {
            using (CreateTestStore())
            {
                var bigUn = new BigUn();
                var fastUns = new[]
                {
                    new FastUn
                    {
                        Name = "First 'un",
                        BigUn = bigUn
                    },
                    new FastUn
                    {
                        Name = "Second 'un",
                        BigUn = bigUn
                    }
                };
                using (var context = CreateContext())
                {
                    context.Database.EnsureCreatedResiliently();

                    // ReSharper disable once CoVariantArrayConversion
                    context.AddRange(fastUns);

                    context.SaveChanges();
                }

                using (var context = CreateContext())
                {
                    Assert.Equal(fastUns.Select(f => f.Name), context.FastUns.OrderBy(f => f.Name).Select(f => f.Name).ToList());
                }
            }
        }

        protected TestStore TestStore { get; set; }

        protected TestStore CreateTestStore()
        {
            TestStore = SqlServerTestStore.GetOrCreate(nameof(MemoryOptimizedTablesTest));
            TestStore.Initialize(null, CreateContext, c => { });
            return TestStore;
        }

        private MemoryOptimizedContext CreateContext() => new MemoryOptimizedContext(Fixture.CreateOptions(TestStore));

        public class MemoryOptimizedTablesFixture : ServiceProviderFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;
        }

        private class MemoryOptimizedContext : DbContext
        {
            public MemoryOptimizedContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<FastUn> FastUns { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<FastUn>(
                        eb =>
                        {
                            eb.ForSqlServerIsMemoryOptimized();
                            eb.HasIndex(e => e.Name).IsUnique();
                            eb.HasOne(e => e.BigUn).WithMany(e => e.FastUns).IsRequired().OnDelete(DeleteBehavior.Restrict);
                        });

                modelBuilder.Entity<BigUn>().ForSqlServerIsMemoryOptimized();
            }
        }

        private class BigUn
        {
            public int Id { get; set; }
            public ICollection<FastUn> FastUns { get; set; }
        }

        private class FastUn
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public BigUn BigUn { get; set; }
        }
    }
}
