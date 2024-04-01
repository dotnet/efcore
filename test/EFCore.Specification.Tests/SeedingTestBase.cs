// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class SeedingTestBase
{
    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Seeding_does_not_leave_context_contaminated(bool async)
    {
        using var context = CreateContextWithEmptyDatabase(async ? "1A" : "1S");
        await TestStore.CleanAsync(context);
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
                using var context = CreateKeylessContextWithEmptyDatabase();
                await TestStore.CleanAsync(context);
                var _ = async
                    ? await context.Database.EnsureCreatedResilientlyAsync()
                    : context.Database.EnsureCreatedResiliently();
            });
        Assert.Equal(CoreStrings.SeedKeylessEntity(nameof(KeylessSeed)), exception.Message);
    }

    protected abstract TestStore TestStore { get; }

    protected abstract SeedingContext CreateContextWithEmptyDatabase(string testId);

    protected virtual KeylessSeedingContext CreateKeylessContextWithEmptyDatabase()
        => new(TestStore.AddProviderOptions(new DbContextOptionsBuilder()).Options);

    protected abstract class SeedingContext : DbContext
    {
        public string TestId { get; }

        protected SeedingContext(string testId)
        {
            TestId = testId;
        }

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

    public class KeylessSeedingContext(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<KeylessSeed>()
                .HasNoKey()
                .HasData(
                    new KeylessSeed { Species = "Apple" },
                    new KeylessSeed { Species = "Orange" }
                );
    }

    public class KeylessSeed
    {
        public string Species { get; set; }
    }
}
