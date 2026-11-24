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

    [Fact]
    public virtual async Task TimeOnly_members_on_properties_converted_to_strings()
    {
        var contextFactory = await InitializeNonSharedTest<ContextTemporalStrings>(
            seed: async c =>
            {
                c.Add(
                    new ContextTemporalStrings.Entity
                    {
                        Id = 1,
                        Time = new TimeOnly(10, 20, 30, 123, 456),
                        CustomTime = new TimeOnly(10, 20, 30)
                    });
                await c.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();

        // The built-in string converter writes the default text format, so these are translated.
        var result = await context.Set<ContextTemporalStrings.Entity>()
            .Where(e => e.Time.Hour == 10 && e.Time.Minute == 20 && e.Time.Second == 30 && e.Time.Microsecond == 456)
            .Select(e => new { Added = e.Time.AddHours(1), e.CustomTime.Hour, CustomAdded = e.CustomTime.AddMinutes(1) })
            .SingleAsync();

        // A custom string format can't be read by the SQL functions, so those are evaluated on the client.
        Assert.Equal(new TimeOnly(11, 20, 30, 123, 456), result.Added);
        Assert.Equal(10, result.Hour);
        Assert.Equal(new TimeOnly(10, 21, 30), result.CustomAdded);
    }

    protected class ContextTemporalStrings(DbContextOptions options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>(
                b =>
                {
                    b.Property(e => e.Time).HasConversion<string>();
                    b.Property(e => e.CustomTime).HasConversion(t => t.ToString("HHmmss"), s => TimeOnly.ParseExact(s, "HHmmss"));
                });

        public class Entity
        {
            public int Id { get; set; }
            public TimeOnly Time { get; set; }
            public TimeOnly CustomTime { get; set; }
        }
    }

    [Fact]
    public virtual async Task DateOnly_TimeOnly_methods_with_default_type_mappings_converted_to_numbers()
    {
        var contextFactory = await InitializeNonSharedTest<ContextDefaultTemporalNumbers>(
            seed: async c =>
            {
                c.Add(
                    new ContextDefaultTemporalNumbers.Entity
                    {
                        Id = 1,
                        Time = new TimeOnly(10, 0, 0, 123, 456),
                        DateTime = new DateTime(2020, 1, 2, 10, 0, 0, 123),
                        Date = new DateOnly(2020, 1, 2)
                    });
                await c.SaveChangesAsync();
            });

        using var context = contextFactory.CreateDbContext();

        // Parameters get the converted default mappings, while the function results are always text.
        var time = new TimeOnly(1, 2);
        var result = await context.Set<ContextDefaultTemporalNumbers.Entity>()
            .Select(e => new
            {
                FromDateTime = TimeOnly.FromDateTime(e.DateTime),
                ToDateTime = e.Date.ToDateTime(e.Time),
                ToDateTimeParameter = e.Date.ToDateTime(time)
            })
            .SingleAsync();

        Assert.Equal(new TimeOnly(10, 0, 0, 123), result.FromDateTime);
        Assert.Equal(new DateTime(2020, 1, 2, 10, 0, 0, 123).AddTicks(4560), result.ToDateTime);
        Assert.Equal(new DateTime(2020, 1, 2, 1, 2, 0), result.ToDateTimeParameter);
    }

    protected class ContextDefaultTemporalNumbers(DbContextOptions options) : DbContext(options)
    {
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.DefaultTypeMapping<TimeOnly>().HasConversion<long>();
            configurationBuilder.DefaultTypeMapping<DateTime>().HasConversion<long>();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Entity>();

        public class Entity
        {
            public int Id { get; set; }
            public TimeOnly Time { get; set; }
            public DateTime DateTime { get; set; }
            public DateOnly Date { get; set; }
        }
    }
}
