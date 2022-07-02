// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class EntitySplittingSqlServerTest : EntitySplittingTestBase
{
    public EntitySplittingSqlServerTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }
    
    [ConditionalFact(Skip = "Entity splitting query Issue #620")]
    public virtual async Task Can_roundtrip_with_triggers()
    {
        await InitializeAsync(modelBuilder =>
            {
                OnModelCreating(modelBuilder);
                
                modelBuilder.Entity<MeterReading>(
                    ob =>
                    {
                        ob.SplitToTable(
                            "MeterReadingDetails", t =>
                            {
                                t.HasTrigger("MeterReadingsDetails_Trigger");
                            });
                    });
            },
            sensitiveLogEnabled: false,
            seed: c =>
                {
                    c.Database.ExecuteSqlRaw($@"
CREATE OR ALTER TRIGGER [MeterReadingsDetails_Trigger]
ON [MeterReadingDetails]
FOR INSERT, UPDATE, DELETE AS
BEGIN
	IF @@ROWCOUNT = 0
		return
END");
                });

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

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;
}
