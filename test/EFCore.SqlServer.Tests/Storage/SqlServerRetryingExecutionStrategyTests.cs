// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Storage;

public class SqlServerRetryingExecutionStrategyTests
{
    [Fact]
    public void GetNextDelay_adds_default_minimum_delay_for_throttling_errors()
    {
        var strategy = new TestSqlServerRetryingExecutionStrategy(CreateContext());

        Assert.Equal(
            TimeSpan.FromSeconds(5),
            strategy.GetNextDelay(SqlExceptionFactory.CreateSqlException(49920)));
    }

    [Fact]
    public void GetNextDelay_adds_configured_minimum_delay_for_throttling_errors()
    {
        var strategy = new TestSqlServerRetryingExecutionStrategy(
            CreateContext(), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(2));

        Assert.Equal(
            TimeSpan.FromSeconds(2),
            strategy.GetNextDelay(SqlExceptionFactory.CreateSqlException(49920)));
    }

    [Fact]
    public void EnableRetryOnFailure_configures_minThrottlingDelay()
    {
        var minThrottlingDelay = TimeSpan.FromSeconds(2);
        var options = new DbContextOptionsBuilder()
            .UseSqlServer("Database=Crunchie", sqlServer => sqlServer.EnableRetryOnFailure(minThrottlingDelay))
            .Options;
        using var context = SqlServerTestHelpers.Instance.CreateContext(options);
        var strategy = context.Database.CreateExecutionStrategy();

        Assert.IsType<SqlServerRetryingExecutionStrategy>(strategy);

        var minThrottlingDelayField = typeof(SqlServerRetryingExecutionStrategy)
            .GetField("_minThrottlingDelay", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(minThrottlingDelayField);
        Assert.Equal(minThrottlingDelay, minThrottlingDelayField.GetValue(strategy));
    }

    [Fact]
    public void EnableRetryOnFailure_configures_minThrottlingDelay_with_retry_settings()
    {
        var minThrottlingDelay = TimeSpan.FromSeconds(2);
        var options = new DbContextOptionsBuilder()
            .UseSqlServer(
                "Database=Crunchie",
                sqlServer => sqlServer.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null,
                    minThrottlingDelay: minThrottlingDelay))
            .Options;
        using var context = SqlServerTestHelpers.Instance.CreateContext(options);
        var strategy = context.Database.CreateExecutionStrategy();
        var minThrottlingDelayField = typeof(SqlServerRetryingExecutionStrategy)
            .GetField("_minThrottlingDelay", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsType<SqlServerRetryingExecutionStrategy>(strategy);
        Assert.NotNull(minThrottlingDelayField);
        Assert.Equal(minThrottlingDelay, minThrottlingDelayField.GetValue(strategy));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void EnableRetryOnFailure_existing_overloads_keep_the_default_minThrottlingDelay(int overload)
    {
        var options = new DbContextOptionsBuilder()
            .UseSqlServer("Database=Crunchie", sqlServer =>
            {
                switch (overload)
                {
                    case 0:
                        sqlServer.EnableRetryOnFailure();
                        break;
                    case 1:
                        sqlServer.EnableRetryOnFailure(maxRetryCount: 3);
                        break;
                    case 2:
                        sqlServer.EnableRetryOnFailure(new[] { 50000 });
                        break;
                    case 3:
                        sqlServer.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
                        break;
                }
            })
            .Options;
        using var context = SqlServerTestHelpers.Instance.CreateContext(options);
        var strategy = context.Database.CreateExecutionStrategy();
        var minThrottlingDelayField = typeof(SqlServerRetryingExecutionStrategy)
            .GetField("_minThrottlingDelay", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.IsType<SqlServerRetryingExecutionStrategy>(strategy);
        Assert.NotNull(minThrottlingDelayField);
        Assert.Equal(TimeSpan.FromSeconds(5), minThrottlingDelayField.GetValue(strategy));
    }

    [Fact]
    public void GetNextDelay_does_not_add_delay_for_throttling_errors_when_configured_delay_is_zero()
    {
        var strategy = new TestSqlServerRetryingExecutionStrategy(
            CreateContext(), TimeSpan.FromSeconds(30), TimeSpan.Zero);

        Assert.Equal(
            TimeSpan.Zero,
            strategy.GetNextDelay(SqlExceptionFactory.CreateSqlException(49920)));
    }

    [Fact]
    public void GetNextDelay_does_not_add_configured_delay_for_non_throttling_errors()
    {
        var minThrottlingDelay = TimeSpan.FromSeconds(2);
        var strategy = new TestSqlServerRetryingExecutionStrategy(
            CreateContext(), TimeSpan.FromSeconds(1), minThrottlingDelay);
        var exception = SqlExceptionFactory.CreateSqlException(4060);

        Assert.Equal(TimeSpan.Zero, strategy.GetNextDelay(exception));

        var delay = strategy.GetNextDelay(exception);
        Assert.True(delay.HasValue);
        Assert.True(delay.Value > TimeSpan.Zero);
        Assert.True(delay.Value < minThrottlingDelay);
    }

    [Fact]
    public void Ctor_throws_when_minThrottlingDelay_is_negative()
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => new TestSqlServerRetryingExecutionStrategy(
                CreateContext(), TimeSpan.FromSeconds(30), TimeSpan.FromTicks(-1)));

    [Fact]
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
                <= (expectedDelays[i].TotalMilliseconds * 0.1) + 1,
                $"Expected: {expectedDelays[i]}; Actual: {delays[i]}");
        }
    }

    protected DbContext CreateContext()
        => SqlServerTestHelpers.Instance.CreateContext();
}
