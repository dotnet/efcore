// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public abstract class EntitySplittingTestBase : NonSharedModelTestBase
{
    protected EntitySplittingTestBase(ITestOutputHelper testOutputHelper)
    {
        //TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    [ConditionalFact(Skip = "Entity splitting query Issue #620")]
    public virtual async Task Can_roundtrip()
    {
        await InitializeAsync(OnModelCreating, sensitiveLogEnabled: false);

        await using (var context = CreateContext())
        {
            var meterReading = new MeterReading { ReadingStatus = MeterReadingStatus.NotAccesible, CurrentRead = "100" };

            context.Add(meterReading);

            TestSqlLoggerFactory.Clear();

            await context.SaveChangesAsync();

            Assert.Empty(TestSqlLoggerFactory.Log.Where(l => l.Level == LogLevel.Warning));
        }
        
        await using (var context = CreateContext())
        {
            var reading = await context.MeterReadings.SingleAsync();

            Assert.Equal(MeterReadingStatus.NotAccesible, reading.ReadingStatus);
            Assert.Equal("100", reading.CurrentRead);
        }
    }

    protected override string StoreName { get; } = "EntitySplittingTest";

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected ContextFactory<EntitySplittingContext> ContextFactory { get; private set; }

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);
    
    protected virtual void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MeterReading>(
            ob =>
            {
                ob.ToTable("MeterReadings");
                ob.SplitToTable(
                    "MeterReadingDetails", t =>
                    {
                        t.Property(o => o.PreviousRead);
                        t.Property(o => o.CurrentRead);
                    });
            });
    }

    protected async Task InitializeAsync(
        Action<ModelBuilder> onModelCreating,
        Action<DbContextOptionsBuilder> onConfiguring = null,
        Action<EntitySplittingContext> seed = null,
        bool sensitiveLogEnabled = true)
        => ContextFactory = await InitializeAsync(
            onModelCreating,
            seed: seed,
            shouldLogCategory: _ => true,
            onConfiguring: options =>
            {
                options.ConfigureWarnings(w => w.Log(RelationalEventId.OptionalDependentWithAllNullPropertiesWarning))
                    .ConfigureWarnings(w => w.Log(RelationalEventId.OptionalDependentWithoutIdentifyingPropertyWarning))
                    .EnableSensitiveDataLogging(sensitiveLogEnabled);
                onConfiguring?.Invoke(options);
            }
        );

    protected virtual EntitySplittingContext CreateContext()
        => ContextFactory.CreateContext();

    public override void Dispose()
    {
        base.Dispose();

        ContextFactory = null;
    }

    protected class EntitySplittingContext : PoolableDbContext
    {
        public EntitySplittingContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<MeterReading> MeterReadings { get; set; }
    }

    protected class MeterReading
    {
        public int Id { get; set; }
        public MeterReadingStatus? ReadingStatus { get; set; }
        public string CurrentRead { get; set; }
        public string PreviousRead { get; set; }
    }

    protected enum MeterReadingStatus
    {
        Running = 0,
        NotAccesible = 2
    }
}
