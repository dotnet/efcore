// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.StoreValueGenerationModel;

namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

public abstract class StoreValueGenerationWithoutOutputSqlServerTestBase<TFixture> : StoreValueGenerationTestBase<TFixture>
    where TFixture : StoreValueGenerationWithoutOutputSqlServerFixture
{
    protected StoreValueGenerationWithoutOutputSqlServerTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Three_Add_use_batched_inserts(bool async)
    {
        await using var context = CreateContext();

        var instances = new StoreValueGenerationData[] { new(), new(), new() };
        context.WithSomeDatabaseGenerated.AddRange(instances[0], instances[1], instances[2]);

        Fixture.ListLoggerFactory.Clear();

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        Assert.Contains(Fixture.ListLoggerFactory.Log, l => l.Id == RelationalEventId.TransactionStarted);
        Assert.Contains(Fixture.ListLoggerFactory.Log, l => l.Id == RelationalEventId.TransactionCommitted);

        Assert.Equal(1, Fixture.ListLoggerFactory.Log.Count(l => l.Id == RelationalEventId.CommandExecuted));

        context.ChangeTracker.Clear();

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            foreach (var instance in instances)
            {
                Assert.Equal(await context.WithSomeDatabaseGenerated.FindAsync(instance.Id), instance);
            }
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Four_Add_use_merge_output_into(bool async)
    {
        await using var context = CreateContext();

        var instances = new StoreValueGenerationData[] { new(), new(), new(), new() };
        context.WithSomeDatabaseGenerated.AddRange(instances[0], instances[1], instances[2], instances[3]);

        Fixture.ListLoggerFactory.Clear();

        if (async)
        {
            await context.SaveChangesAsync();
        }
        else
        {
            context.SaveChanges();
        }

        Assert.Contains(Fixture.ListLoggerFactory.Log, l => l.Id == RelationalEventId.TransactionStarted);
        Assert.Contains(Fixture.ListLoggerFactory.Log, l => l.Id == RelationalEventId.TransactionCommitted);

        Assert.Equal(1, Fixture.ListLoggerFactory.Log.Count(l => l.Id == RelationalEventId.CommandExecuted));

        context.ChangeTracker.Clear();

        using (Fixture.TestSqlLoggerFactory.SuspendRecordingEvents())
        {
            foreach (var instance in instances)
            {
                Assert.Equal(await context.WithSomeDatabaseGenerated.FindAsync(instance.Id), instance);
            }
        }

        AssertSql(
            """
@p0='0'
@p1='0'
@p2='0'
@p3='0'

SET NOCOUNT ON;
DECLARE @inserted0 TABLE ([Id] int, [_Position] [int]);
MERGE [WithSomeDatabaseGenerated] USING (
VALUES (@p0, 0),
(@p1, 1),
(@p2, 2),
(@p3, 3)) AS i ([Data2], _Position) ON 1=0
WHEN NOT MATCHED THEN
INSERT ([Data2])
VALUES (i.[Data2])
OUTPUT INSERTED.[Id], i._Position
INTO @inserted0;

SELECT [t].[Id], [t].[Data1] FROM [WithSomeDatabaseGenerated] t
INNER JOIN @inserted0 i ON ([t].[Id] = [i].[Id])
ORDER BY [i].[_Position];
""");
    }
}
