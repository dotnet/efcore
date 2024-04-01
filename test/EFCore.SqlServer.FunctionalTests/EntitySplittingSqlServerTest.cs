// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class EntitySplittingSqlServerTest(ITestOutputHelper testOutputHelper) : EntitySplittingTestBase(testOutputHelper)
{
    [ConditionalFact]
    public virtual async Task Can_roundtrip_with_triggers()
    {
        await InitializeAsync(
            modelBuilder =>
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
            seed: c => c.Database.ExecuteSqlRawAsync(
                @"
CREATE OR ALTER TRIGGER [MeterReadingsDetails_Trigger]
ON [MeterReadingDetails]
FOR INSERT, UPDATE, DELETE AS
BEGIN
	IF @@ROWCOUNT = 0
		return
END"));

        await using (var context = CreateContext())
        {
            var meterReading = new MeterReading { ReadingStatus = MeterReadingStatus.NotAccesible, CurrentRead = "100" };

            await context.AddAsync(meterReading);

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

    public override async Task Can_roundtrip()
    {
        await base.Can_roundtrip();

        AssertSql(
            """
@p0='2' (Nullable = true)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [MeterReadings] ([ReadingStatus])
OUTPUT INSERTED.[Id]
VALUES (@p0);
""",
            //
            """
@p1='1'
@p2='100' (Size = 4000)
@p3=NULL (Size = 4000)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
INSERT INTO [MeterReadingDetails] ([Id], [CurrentRead], [PreviousRead])
VALUES (@p1, @p2, @p3);
""",
            //
            """
SELECT TOP(2) [m].[Id], [m0].[CurrentRead], [m0].[PreviousRead], [m].[ReadingStatus]
FROM [MeterReadings] AS [m]
INNER JOIN [MeterReadingDetails] AS [m0] ON [m].[Id] = [m0].[Id]
""");
    }

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;
}
