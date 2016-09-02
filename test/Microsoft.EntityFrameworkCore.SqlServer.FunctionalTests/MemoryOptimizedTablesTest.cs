// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    [SqlServerCondition(SqlServerCondition.SupportsMemoryOptimized)]
    public class MemoryOptimizedTablesTest
    {
        [ConditionalFact]
        public void Can_create_memoryOptimized_table()
        {
            var bigUn = new BigUn();
            var fastUns = new[] { new FastUn { Name = "First 'un", BigUn = bigUn }, new FastUn { Name = "Second 'un", BigUn = bigUn } };
            using (var context = new MemoryOptimizedContext())
            {
                context.Database.EnsureClean();

                context.AddRange(fastUns);

                context.SaveChanges();
            }

            using (var context = new MemoryOptimizedContext())
            {
                Assert.Equal(fastUns.Select(f => f.Name), context.FastUns.OrderBy(f => f.Name).Select(f => f.Name).ToList());

                context.Database.EnsureDeleted();
            }
        }

        private class MemoryOptimizedContext : DbContext
        {
            public DbSet<FastUn> FastUns { get; set; }
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString("MemoryOptimizedTest"));

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<FastUn>(eb =>
                    {
                        eb.ForSqlServerIsMemoryOptimized();
                        eb.HasIndex(e => e.Name).IsUnique();
                        eb.HasOne(e => e.BigUn).WithMany(e => e.FastUns).IsRequired(true).OnDelete(DeleteBehavior.Restrict);
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
