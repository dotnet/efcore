// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class AdHocAdvancedMappingsQuerySqliteTest(NonSharedFixture fixture) : AdHocAdvancedMappingsQueryRelationalTestBase(fixture)
{
    protected override ITestStoreFactory NonSharedTestStoreFactory
        => SqliteTestStoreFactory.Instance;

    [Fact]
    public virtual async Task DateOnly_TimeOnly_members_on_properties_converted_to_numbers()
    {
        var contextFactory = await InitializeNonSharedTest<ContextTemporalNumbers>(
            seed: async c =>
            {
                c.Add(
                    new ContextTemporalNumbers.Entity
                    {
                        Id = 1,
                        Time = new TimeOnly(10, 0, 0, 123, 456),
                        DateTime = new DateTime(2020, 1, 2, 10, 0, 0, 123),
                        Date = new DateOnly(2020, 1, 2)
                    });
                await c.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();

        // The SQL functions would misread the stored numbers, so these must not be translated.
        var result = await context.Set<ContextTemporalNumbers.Entity>()
            .Select(e => new
            {
                e.Time.Millisecond,
                e.Time.Microsecond,
                Added = e.Time.AddHours(1),
                FromDateTime = TimeOnly.FromDateTime(e.DateTime),
                ToDateTime = e.Date.ToDateTime(new TimeOnly(1, 2))
            })
            .SingleAsync();

        Assert.Equal(123, result.Millisecond);
        Assert.Equal(456, result.Microsecond);
        Assert.Equal(new TimeOnly(11, 0, 0, 123, 456), result.Added);
        Assert.Equal(new TimeOnly(10, 0, 0, 123), result.FromDateTime);
        Assert.Equal(new DateTime(2020, 1, 2, 1, 2, 0), result.ToDateTime);
    }

    protected class ContextTemporalNumbers(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(
                b =>
                {
                    b.Property(e => e.Time).HasConversion<long>();
                    b.Property(e => e.DateTime).HasConversion<long>();
                    b.Property(e => e.Date).HasConversion(d => d.DayNumber, n => DateOnly.FromDayNumber(n));
                });

        public class Entity
        {
            public int Id { get; set; }
            public TimeOnly Time { get; set; }
            public DateTime DateTime { get; set; }
            public DateOnly Date { get; set; }
        }
    }
}
