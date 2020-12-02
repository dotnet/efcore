// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SeedingSqlServerTest : SeedingTestBase
    {
        protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
        {
            var context = new SeedingSqlServerContext(testId);

            context.Database.EnsureClean();

            return context;
        }

        protected KeylessSeedingContext CreateKeylessContextWithEmptyDatabase(string testId)
        {
            var context = new KeylessSeedingContext(testId);

            context.Database.EnsureClean();

            return context;
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Seeding_keyless_entity_success(bool async)
        {
            using var context = CreateKeylessContextWithEmptyDatabase(async ? "1A" : "1S");
            var _ = async
                ? await context.Database.EnsureCreatedResilientlyAsync()
                : context.Database.EnsureCreatedResiliently();

            Assert.Empty(context.ChangeTracker.Entries());

            var seeds = context.Set<KeylessSeed>().OrderBy(e => e.Species).ToList();
            Assert.Equal(2, seeds.Count);
            Assert.Equal("Apple", seeds[0].Species);
            Assert.Equal("Orange", seeds[1].Species);
        }

        protected class SeedingSqlServerContext : SeedingContext
        {
            public SeedingSqlServerContext(string testId)
                : base(testId)
            {
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString($"Seeds{TestId}"));
        }

        protected class KeylessSeedingContext : DbContext
        {
            public string TestId { get; }

            public KeylessSeedingContext(string testId)
                => TestId = testId;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<KeylessSeed>()
                    .HasNoKey()
                    .HasData(
                    new Seed { Species = "Apple" },
                    new Seed { Species = "Orange" }
                );

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseSqlServer(SqlServerTestStore.CreateConnectionString($"Seeds{TestId}"));
        }

        protected class KeylessSeed
        {
            public string Species { get; set; }
        }
    }
}
