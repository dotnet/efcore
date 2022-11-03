// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Storage;

public class SqlServerRetryingExecutionStrategyTests
{
    [ConditionalFact]
    public void GetNextDelay_returns_shorter_delay_for_InMemory_transient_errors()
    {
        var strategy = new TestSqlServerRetryingExecutionStrategy(CreateContext());
        var inMemoryError = SqlExceptionFactory.CreateSqlException(41302);
        var delays = new List<TimeSpan>();
        var delay = strategy.GetNextDelay(inMemoryError);
        while (delay != null)
        {
            delays.Add(delay.Value);
            delay = strategy.GetNextDelay(inMemoryError);
        }

        var expectedDelays = new List<TimeSpan>
        {
            TimeSpan.FromMilliseconds(0),
            TimeSpan.FromMilliseconds(1),
            TimeSpan.FromMilliseconds(3),
            TimeSpan.FromMilliseconds(7),
            TimeSpan.FromMilliseconds(15),
            TimeSpan.FromMilliseconds(31)
        };

        Assert.Equal(expectedDelays.Count, delays.Count);
        for (var i = 0; i < expectedDelays.Count; i++)
        {
            Assert.True(
                Math.Abs((delays[i] - expectedDelays[i]).TotalMilliseconds)
                <= expectedDelays[i].TotalMilliseconds * 0.1 + 1,
                $"Expected: {expectedDelays[i]}; Actual: {delays[i]}");
        }
    }

    protected DbContext CreateContext()
        => SqlServerTestHelpers.Instance.CreateContext();
}
