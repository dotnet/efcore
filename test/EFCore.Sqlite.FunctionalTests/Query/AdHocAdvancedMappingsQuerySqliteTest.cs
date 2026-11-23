// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocAdvancedMappingsQuerySqliteTest(NonSharedFixture fixture) : AdHocAdvancedMappingsQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    [Fact]
    public virtual async Task TimeSpan_members_on_property_converted_to_ticks()
    {
        var contextFactory = await InitializeNonSharedTest<ContextTimeSpanTicks>(
            seed: async c =>
            {
                c.Add(new ContextTimeSpanTicks.Entity { Id = 1, Duration = TimeSpan.FromTicks(-3) });
                await c.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();

        // ef_ticks() would read the stored ticks as days, so these must not be translated.
        var result = await context.Set<ContextTimeSpanTicks.Entity>()
            .Select(e => new { e.Duration.Ticks, Duration = e.Duration.Duration(), Negated = e.Duration.Negate() })
            .SingleAsync();

        Assert.Equal(-3, result.Ticks);
        Assert.Equal(TimeSpan.FromTicks(3), result.Duration);
        Assert.Equal(TimeSpan.FromTicks(3), result.Negated);
    }

    protected class ContextTimeSpanTicks(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>().Property(e => e.Duration).HasConversion<long>();

        public class Entity
        {
            public int Id { get; set; }
            public TimeSpan Duration { get; set; }
        }
    }
}
