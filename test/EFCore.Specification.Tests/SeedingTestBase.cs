// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class SeedingTestBase
    {
        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Seeding_does_not_leave_context_contaminated(bool async)
        {
            using var context = CreateContextWithEmptyDatabase(async ? "1A" : "1S");
            var _ = async
                ? await context.Database.EnsureCreatedResilientlyAsync()
                : context.Database.EnsureCreatedResiliently();

            Assert.Empty(context.ChangeTracker.Entries());

            var seeds = context.Set<Seed>().OrderBy(e => e.Id).ToList();
            Assert.Equal(2, seeds.Count);
            Assert.Equal(321, seeds[0].Id);
            Assert.Equal("Apple", seeds[0].Species);
            Assert.Equal(322, seeds[1].Id);
            Assert.Equal("Orange", seeds[1].Species);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Seeding_keyless_entity_throws_exception(bool async)
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                async () =>
                {
                    using var context = CreateKeylessContextWithEmptyDatabase(async ? "1A" : "1S");
                    var _ = async
                        ? await context.Database.EnsureCreatedResilientlyAsync()
                        : context.Database.EnsureCreatedResiliently();

                    Assert.Empty(context.ChangeTracker.Entries());

                    var seeds = context.Set<KeylessSeed>().OrderBy(e => e.Species).ToList();

                    Assert.Empty(seeds);
                });
            Assert.Equal(CoreStrings.SeedKeylessEntity(nameof(KeylessSeed)), exception.Message);
        }

        protected abstract SeedingContext CreateContextWithEmptyDatabase(string testId);

        protected abstract KeylessSeedingContext CreateKeylessContextWithEmptyDatabase(string testId);

        protected abstract class SeedingContext : DbContext
        {
            public string TestId { get; }

            protected SeedingContext(string testId)
                => TestId = testId;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Seed>().HasData(
                    new Seed { Id = 321, Species = "Apple" },
                    new Seed { Id = 322, Species = "Orange" }
                );
        }

        protected class Seed
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string Species { get; set; }
        }

        protected abstract class KeylessSeedingContext : DbContext
        {
            public string TestId { get; }

            protected KeylessSeedingContext(string testId)
                => TestId = testId;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<KeylessSeed>()
                    .HasNoKey()
                    .HasData(
                    new KeylessSeed { Species = "Apple" },
                    new KeylessSeed { Species = "Orange" }
                );
        }

        protected class KeylessSeed
        {
            public string Species { get; set; }
        }
    }
}
