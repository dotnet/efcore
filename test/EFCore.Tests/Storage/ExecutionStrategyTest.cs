// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;

// ReSharper disable AccessToModifiedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Storage;

public class ExecutionStrategyTest : IDisposable
{
    private readonly DbContext Context;

    public ExecutionStrategyTest()
    {
        Context = CreateContext();
    }

    public void Dispose()
        => Context.Dispose();

    private TestExecutionStrategy CreateFailOnRetryStrategy()
        => new(
            Context,
            shouldRetryOn: e =>
            {
                Assert.True(false);
                return false;
            },
            getNextDelay: e =>
            {
                Assert.True(false);
                return null;
            });

    [ConditionalFact]
    public void GetNextDelay_returns_the_expected_default_sequence()
    {
        var strategy = new TestExecutionStrategy(Context);
        var delays = new List<TimeSpan>();
        var delay = strategy.GetNextDelayBase(new Exception());
        while (delay != null)
        {
            delays.Add(delay.Value);
            delay = strategy.GetNextDelayBase(new Exception());
        }

        var expectedDelays = new List<TimeSpan>
        {
            TimeSpan.FromSeconds(0),
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(7),
            TimeSpan.FromSeconds(15),
            TimeSpan.FromSeconds(31)
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

    [ConditionalFact]
    public void RetriesOnFailure_returns_true()
    {
        var mockExecutionStrategy = new TestExecutionStrategy(Context);

        Assert.True(mockExecutionStrategy.RetriesOnFailure);
    }

    [ConditionalFact]
    public void Execute_Action_throws_for_an_existing_transaction()
        => Execute_throws_for_an_existing_transaction(e => e.Execute(() => { }));

    [ConditionalFact]
    public void Execute_Func_throws_for_an_existing_transaction()
        => Execute_throws_for_an_existing_transaction(e => e.Execute(() => 1));

    private void Execute_throws_for_an_existing_transaction(Action<ExecutionStrategy> execute)
    {
        var mockExecutionStrategy = new TestExecutionStrategy(Context);
        using (Context.Database.BeginTransaction())
        {
            Assert.Equal(
                CoreStrings.ExecutionStrategyExistingTransaction(
                    mockExecutionStrategy.GetType().Name, "DbContext.Database.CreateExecutionStrategy()"),
                Assert.Throws<InvalidOperationException>(
                        () => execute(mockExecutionStrategy))
                    .Message);
        }
    }

    [ConditionalFact]
    public void Execute_Action_throws_for_an_ambient_transaction()
        => Execute_throws_for_an_ambient_transaction(e => e.Execute(() => { }));

    [ConditionalFact]
    public void Execute_Func_throws_for_an_ambient_transaction()
        => Execute_throws_for_an_ambient_transaction(e => e.Execute(() => 1));

    private void Execute_throws_for_an_ambient_transaction(Action<ExecutionStrategy> execute)
    {
        var mockExecutionStrategy = new TestExecutionStrategy(Context);
        using (TestStore.CreateTransactionScope())
        {
            Assert.Equal(
                CoreStrings.ExecutionStrategyExistingTransaction(
                    mockExecutionStrategy.GetType().Name, "DbContext.Database.CreateExecutionStrategy()"),
                Assert.Throws<InvalidOperationException>(
                        () => execute(mockExecutionStrategy))
                    .Message);
        }
    }

    [ConditionalFact]
    public void Execute_Action_throws_for_an_enlisted_transaction()
        => Execute_throws_for_an_enlisted_transaction(e => e.Execute(() => { }));

    [ConditionalFact]
    public void Execute_Func_throws_for_an_enlisted_transaction()
        => Execute_throws_for_an_enlisted_transaction(e => e.Execute(() => 1));

    private void Execute_throws_for_an_enlisted_transaction(Action<ExecutionStrategy> execute)
    {
        var mockExecutionStrategy = new TestExecutionStrategy(Context);
        using var t = new CommittableTransaction();
        Context.Database.EnlistTransaction(t);

        Assert.Equal(
            CoreStrings.ExecutionStrategyExistingTransaction(
                mockExecutionStrategy.GetType().Name, "DbContext.Database.CreateExecutionStrategy()"),
            Assert.Throws<InvalidOperationException>(
                    () => execute(mockExecutionStrategy))
                .Message);
    }

    [ConditionalFact]
    public void Execute_Action_does_not_throw_when_invoked_twice()
        => Execute_does_not_throw_when_invoked_twice((e, f) => e.Execute(() => f()));

    [ConditionalFact]
    public void Execute_Func_does_not_throw_when_invoked_twice()
        => Execute_does_not_throw_when_invoked_twice((e, f) => e.Execute(f));

    private void Execute_does_not_throw_when_invoked_twice(Action<ExecutionStrategy, Func<int>> execute)
    {
        var executed = false;

        var executionStrategyMock = new TestExecutionStrategy(
            Context,
            shouldRetryOn: e => e is ArgumentOutOfRangeException);

        for (var i = 0; i < 2; i++)
        {
            execute(
                executionStrategyMock, () =>
                {
                    if (!executed)
                    {
                        executed = true;
                        throw new ArgumentOutOfRangeException();
                    }

                    return 0;
                });

            Assert.True(executed);
            executed = false;
        }
    }

    [ConditionalFact]
    public void Execute_Action_does_not_throw_for_an_existing_transaction_if_RetryOnFailure_disabled()
        => Execute_does_not_throw_for_an_existing_transaction_if_RetryOnFailure_disabled((e, f) => e.Execute(() => f()));

    [ConditionalFact]
    public void Execute_Func_does_not_throw_for_an_existing_transaction_if_RetryOnFailure_disabled()
        => Execute_does_not_throw_for_an_existing_transaction_if_RetryOnFailure_disabled((e, f) => e.Execute(f));

    private void Execute_does_not_throw_for_an_existing_transaction_if_RetryOnFailure_disabled(
        Action<ExecutionStrategy, Func<int>> execute)
    {
        using var context1 = CreateContext();
        using var context2 = CreateContext();

        var mockExecutionStrategy1 = new TestExecutionStrategy(context1, retryCount: 0);
        var mockExecutionStrategy2 = new TestExecutionStrategy(context2, retryCount: 0);

        using var tran1 = context1.Database.BeginTransaction();
        using var tran2 = context2.Database.BeginTransaction();

        var executed1 = false;
        var executed2 = false;

        execute(
            mockExecutionStrategy1, () =>
            {
                executed1 = true;
                return 0;
            });

        execute(
            mockExecutionStrategy2, () =>
            {
                executed2 = true;
                return 0;
            });

        tran1.Commit();
        tran2.Commit();

        Assert.True(executed1);
        Assert.True(executed2);
    }

    [ConditionalFact]
    public void Execute_Action_doesnt_retry_if_successful()
        => Execute_doesnt_retry_if_successful((e, f) => e.Execute(() => f()));

    [ConditionalFact]
    public void Execute_Func_doesnt_retry_if_successful()
        => Execute_doesnt_retry_if_successful((e, f) => e.Execute(f));

    private void Execute_doesnt_retry_if_successful(Action<ExecutionStrategy, Func<int>> execute)
    {
        var executionCount = 0;
        execute(CreateFailOnRetryStrategy(), () => executionCount++);

        Assert.Equal(1, executionCount);
    }

    [ConditionalFact]
    public void Execute_Action_retries_until_successful()
        => Execute_retries_until_successful((e, f) => e.Execute(() => f()));

    [ConditionalFact]
    public void Execute_Func_retries_until_successful()
        => Execute_retries_until_successful((e, f) => e.Execute(f));

    private void Execute_retries_until_successful(Action<ExecutionStrategy, Func<int>> execute)
    {
        var executionStrategyMock = new TestExecutionStrategy(
            Context,
            shouldRetryOn: e => e is ArgumentOutOfRangeException,
            getNextDelay: e => TimeSpan.FromTicks(0));

        var executionCount = 0;

        execute(
            executionStrategyMock, () =>
            {
                if (executionCount++ < 3)
                {
                    throw new DbUpdateException("", new ArgumentOutOfRangeException());
                }

                return executionCount;
            });

        Assert.Equal(4, executionCount);
    }

    [ConditionalFact]
    public void Execute_Action_retries_until_not_retriable_exception_is_thrown()
        => Execute_retries_until_not_retriable_exception_is_thrown((e, f) => e.Execute(() => f()));

    [ConditionalFact]
    public void Execute_Func_retries_until_not_retriable_exception_is_thrown()
        => Execute_retries_until_not_retriable_exception_is_thrown((e, f) => e.Execute(f));

    private void Execute_retries_until_not_retriable_exception_is_thrown(Action<ExecutionStrategy, Func<int>> execute)
    {
        var executionStrategyMock = new TestExecutionStrategy(
            Context,
            shouldRetryOn: e => e is ArgumentOutOfRangeException,
            getNextDelay: e => TimeSpan.FromTicks(0));

        var executionCount = 0;

        Assert.Throws<ArgumentNullException>(
            () =>
                execute(
                    executionStrategyMock, () =>
                    {
                        if (executionCount++ < 3)
                        {
                            throw new ArgumentOutOfRangeException();
                        }

                        throw new ArgumentNullException();
                    }));

        Assert.Equal(4, executionCount);
    }

    [ConditionalFact]
    public void Execute_Action_retries_until_limit_is_reached()
        => Execute_retries_until_limit_is_reached((e, f) => e.Execute(() => f()));

    [ConditionalFact]
    public void Execute_Func_retries_until_limit_is_reached()
        => Execute_retries_until_limit_is_reached((e, f) => e.Execute(f));

    private void Execute_retries_until_limit_is_reached(Action<ExecutionStrategy, Func<int>> execute)
    {
        var executionCount = 0;

        var executionStrategyMock = new TestExecutionStrategy(
            Context,
            retryCount: 2,
            shouldRetryOn: e => e is ArgumentOutOfRangeException,
            getNextDelay: e => TimeSpan.FromTicks(0));

        Assert.IsType<ArgumentOutOfRangeException>(
            Assert.Throws<RetryLimitExceededException>(
                    () =>
                        execute(
                            executionStrategyMock, () =>
                            {
                                if (executionCount++ < 3)
                                {
                                    throw new ArgumentOutOfRangeException();
                                }

                                Assert.True(false);
                                return 0;
                            }))
                .InnerException);

        Assert.Equal(3, executionCount);
    }

    [ConditionalFact]
    public Task ExecuteAsync_Action_throws_for_an_existing_transaction()
        => ExecuteAsync_throws_for_an_existing_transaction(e => e.ExecuteAsync(() => (Task)Task.FromResult(1)));

    [ConditionalFact]
    public Task ExecuteAsync_Func_throws_for_an_existing_transaction()
        => ExecuteAsync_throws_for_an_existing_transaction(e => e.ExecuteAsync(ct => Task.FromResult(1), CancellationToken.None));

    private async Task ExecuteAsync_throws_for_an_existing_transaction(Func<ExecutionStrategy, Task> executeAsync)
    {
        var mockExecutionStrategy = new TestExecutionStrategy(Context);
        using (Context.Database.BeginTransaction())
        {
            Assert.Equal(
                CoreStrings.ExecutionStrategyExistingTransaction(
                    mockExecutionStrategy.GetType().Name, "DbContext.Database.CreateExecutionStrategy()"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => executeAsync(mockExecutionStrategy))).Message);
        }
    }

    [ConditionalFact]
    public async Task ExecuteAsync_Action_throws_for_an_ambient_transaction()
        => await ExecuteAsync_throws_for_an_ambient_transaction(e => e.ExecuteAsync(() => (Task)Task.FromResult(1)));

    [ConditionalFact]
    public async Task ExecuteAsync_Func_throws_for_an_ambient_transaction()
        => await ExecuteAsync_throws_for_an_ambient_transaction(e => e.ExecuteAsync(ct => Task.FromResult(1), CancellationToken.None));

    private async Task ExecuteAsync_throws_for_an_ambient_transaction(Func<ExecutionStrategy, Task> executeAsync)
    {
        var mockExecutionStrategy = new TestExecutionStrategy(Context);
        using (TestStore.CreateTransactionScope())
        {
            Assert.Equal(
                CoreStrings.ExecutionStrategyExistingTransaction(
                    mockExecutionStrategy.GetType().Name, "DbContext.Database.CreateExecutionStrategy()"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    () => executeAsync(mockExecutionStrategy)))
                .Message);
        }
    }

    [ConditionalFact]
    public async Task ExecuteAsync_Action_throws_for_an_enlisted_transaction()
        => await ExecuteAsync_throws_for_an_enlisted_transaction(e => e.ExecuteAsync(() => (Task)Task.FromResult(1)));

    [ConditionalFact]
    public async Task ExecuteAsync_Func_throws_for_an_enlisted_transaction()
        => await ExecuteAsync_throws_for_an_enlisted_transaction(e => e.ExecuteAsync(ct => Task.FromResult(1), CancellationToken.None));

    private async Task ExecuteAsync_throws_for_an_enlisted_transaction(Func<ExecutionStrategy, Task> executeAsync)
    {
        var mockExecutionStrategy = new TestExecutionStrategy(Context);
        using var t = new CommittableTransaction();
        Context.Database.EnlistTransaction(t);

        Assert.Equal(
            CoreStrings.ExecutionStrategyExistingTransaction(
                mockExecutionStrategy.GetType().Name, "DbContext.Database.CreateExecutionStrategy()"),
            (await Assert.ThrowsAsync<InvalidOperationException>(
                () => executeAsync(mockExecutionStrategy)))
            .Message);
    }

    [ConditionalFact]
    public Task ExecuteAsync_Action_does_not_throw_when_invoked_twice()
        => ExecuteAsync_does_not_throw_when_invoked_twice((e, f) => e.ExecuteAsync(() => (Task)f(CancellationToken.None)));

    [ConditionalFact]
    public Task ExecuteAsync_Func_does_not_throw_when_invoked_twice()
        => ExecuteAsync_does_not_throw_when_invoked_twice((e, f) => e.ExecuteAsync(f, CancellationToken.None));

    private async Task ExecuteAsync_does_not_throw_when_invoked_twice(
        Func<ExecutionStrategy, Func<CancellationToken, Task<int>>, Task> executeAsync)
    {
        var executed = false;

        var executionStrategyMock = new TestExecutionStrategy(
            Context,
            shouldRetryOn: e => e is ArgumentOutOfRangeException);

        for (var i = 0; i < 2; i++)
        {
            await executeAsync(
                executionStrategyMock, ct =>
                {
                    if (!executed)
                    {
                        executed = true;
                        throw new ArgumentOutOfRangeException();
                    }

                    return Task.FromResult(0);
                });

            Assert.True(executed);
            executed = false;
        }
    }

    [ConditionalFact]
    public async Task ExecuteAsync_Action_does_not_throw_for_an_existing_transaction_if_RetryOnFailure_disabled()
        => await ExecuteAsync_does_not_throw_for_an_existing_transaction_if_RetryOnFailure_disabled(
            (e, f) => e.ExecuteAsync(() => (Task)f(CancellationToken.None)));

    [ConditionalFact]
    public async Task ExecuteAsync_Func_does_not_throw_for_an_existing_transaction_if_RetryOnFailure_disabled()
        => await ExecuteAsync_does_not_throw_for_an_existing_transaction_if_RetryOnFailure_disabled(
            (e, f) => e.ExecuteAsync(f, CancellationToken.None));

    private async Task ExecuteAsync_does_not_throw_for_an_existing_transaction_if_RetryOnFailure_disabled(
        Func<ExecutionStrategy, Func<CancellationToken, Task<int>>, Task> executeAsync)
    {
        await using var context1 = CreateContext();
        await using var context2 = CreateContext();

        var mockExecutionStrategy1 = new TestExecutionStrategy(context1, retryCount: 0);
        var mockExecutionStrategy2 = new TestExecutionStrategy(context2, retryCount: 0);

        await using var tran1 = context1.Database.BeginTransaction();
        await using var tran2 = context2.Database.BeginTransaction();

        var executed1 = false;
        var executed2 = false;

        await executeAsync(
            mockExecutionStrategy1, ct =>
            {
                executed1 = true;
                return Task.FromResult(0);
            });

        await executeAsync(
            mockExecutionStrategy2, ct =>
            {
                executed2 = true;
                return Task.FromResult(0);
            });

        await tran1.CommitAsync();
        await tran2.CommitAsync();

        Assert.True(executed1);
        Assert.True(executed2);
    }

    [ConditionalFact]
    public Task ExecuteAsync_Action_doesnt_retry_if_successful()
        => ExecuteAsync_doesnt_retry_if_successful((e, f) => e.ExecuteAsync(ct => (Task)f(ct), CancellationToken.None));

    [ConditionalFact]
    public Task ExecuteAsync_Func_doesnt_retry_if_successful()
        => ExecuteAsync_doesnt_retry_if_successful((e, f) => e.ExecuteAsync(f, CancellationToken.None));

    private async Task ExecuteAsync_doesnt_retry_if_successful(
        Func<ExecutionStrategy, Func<CancellationToken, Task<int>>, Task> executeAsync)
    {
        var executionCount = 0;
        await executeAsync(CreateFailOnRetryStrategy(), ct => Task.FromResult(executionCount++));

        Assert.Equal(1, executionCount);
    }

    [ConditionalFact]
    public Task ExecuteAsync_Action_retries_until_successful()
        => ExecuteAsync_retries_until_successful((e, f) => e.ExecuteAsync(ct => (Task)f(ct), CancellationToken.None));

    [ConditionalFact]
    public Task ExecuteAsync_Func_retries_until_successful()
        => ExecuteAsync_retries_until_successful((e, f) => e.ExecuteAsync(f, CancellationToken.None));

    private async Task ExecuteAsync_retries_until_successful(
        Func<ExecutionStrategy, Func<CancellationToken, Task<int>>, Task> executeAsync)
    {
        var executionStrategyMock = new TestExecutionStrategy(
            Context,
            shouldRetryOn: e => e is ArgumentOutOfRangeException,
            getNextDelay: e => TimeSpan.FromTicks(0));

        var executionCount = 0;

        await executeAsync(
            executionStrategyMock, ct =>
            {
                if (executionCount++ < 3)
                {
                    throw new DbUpdateException("", new ArgumentOutOfRangeException());
                }

                return Task.FromResult(executionCount);
            });

        Assert.Equal(4, executionCount);
    }

    [ConditionalFact]
    public Task ExecuteAsync_Action_retries_until_not_retrieable_exception_is_thrown()
        => ExecuteAsync_retries_until_not_retrieable_exception_is_thrown(
            (e, f) => e.ExecuteAsync(ct => (Task)f(ct), CancellationToken.None));

    [ConditionalFact]
    public Task ExecuteAsync_Func_retries_until_not_retrieable_exception_is_thrown()
        => ExecuteAsync_retries_until_not_retrieable_exception_is_thrown((e, f) => e.ExecuteAsync(f, CancellationToken.None));

    private async Task ExecuteAsync_retries_until_not_retrieable_exception_is_thrown(
        Func<ExecutionStrategy, Func<CancellationToken, Task<int>>, Task> executeAsync)
    {
        var executionStrategyMock = new TestExecutionStrategy(
            Context,
            shouldRetryOn: e => e is ArgumentOutOfRangeException,
            getNextDelay: e => TimeSpan.FromTicks(0));

        var executionCount = 0;

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => executeAsync(
                executionStrategyMock, ct =>
                {
                    if (executionCount++ < 3)
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                    throw new ArgumentNullException();
                }));

        Assert.Equal(4, executionCount);
    }

    [ConditionalFact]
    public Task ExecuteAsync_Action_retries_until_limit_is_reached()
        => ExecuteAsync_retries_until_limit_is_reached((e, f) => e.ExecuteAsync(ct => (Task)f(ct), CancellationToken.None));

    [ConditionalFact]
    public Task ExecuteAsync_Func_retries_until_limit_is_reached()
        => ExecuteAsync_retries_until_limit_is_reached((e, f) => e.ExecuteAsync(f, CancellationToken.None));

    private async Task ExecuteAsync_retries_until_limit_is_reached(
        Func<ExecutionStrategy, Func<CancellationToken, Task<int>>, Task> executeAsync)
    {
        var executionCount = 0;

        var executionStrategyMock = new TestExecutionStrategy(
            Context,
            retryCount: 2,
            shouldRetryOn: e => e is ArgumentOutOfRangeException,
            getNextDelay: e => TimeSpan.FromTicks(0));

        // ReSharper disable once PossibleNullReferenceException
        Assert.IsType<ArgumentOutOfRangeException>(
            (await Assert.ThrowsAsync<RetryLimitExceededException>(
                () =>
                    executeAsync(
                        executionStrategyMock, ct =>
                        {
                            if (executionCount++ < 3)
                            {
                                throw new DbUpdateException("", new ArgumentOutOfRangeException());
                            }

                            Assert.True(false);
                            return Task.FromResult(0);
                        }))).InnerException.InnerException);

        Assert.Equal(3, executionCount);
    }

    [ConditionalFact]
    public void ShouldRetryOn_does_not_get_null_on_DbUpdateConcurrencyException()
    {
        var executionStrategyMock = new TestExecutionStrategy(
            Context,
            shouldRetryOn: e =>
            {
                Assert.IsType<DbUpdateConcurrencyException>(e);
                return true;
            },
            getNextDelay: e => TimeSpan.FromTicks(0));

        var executionCount = 0;

        executionStrategyMock.Execute(
            () =>
            {
                if (executionCount++ < 1)
                {
                    throw new DbUpdateConcurrencyException("");
                }
            });

        Assert.Equal(2, executionCount);
    }

    [ConditionalFact]
    public async Task ExecuteAsync_preserves_synchronization_context_across_retries()
    {
        var mockExecutionStrategy = new TestExecutionStrategy(Context, shouldRetryOn: e => e is DbUpdateConcurrencyException);

        var origSyncContext = SynchronizationContext.Current;
        using var syncContext = new SingleThreadSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(syncContext);

        try
        {
            var executionCount = 0;

            await mockExecutionStrategy.ExecuteAsync(
                async _ =>
                {
                    Assert.Same(syncContext, SynchronizationContext.Current);
                    await Task.Yield();
                    if (executionCount++ < 1)
                    {
                        throw new DbUpdateConcurrencyException("");
                    }
                }, cancellationToken: default);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(origSyncContext);
        }
    }

    protected DbContext CreateContext()
        => InMemoryTestHelpers.Instance.CreateContext(
            InMemoryTestHelpers.Instance.CreateServiceProvider(
                new ServiceCollection()
                    .AddScoped<IDbContextTransactionManager, TestInMemoryTransactionManager>()),
            InMemoryTestHelpers.Instance.CreateOptions());

    public class TestExecutionStrategy : ExecutionStrategy
    {
        private readonly Func<Exception, bool> _shouldRetryOn;
        private readonly Func<Exception, TimeSpan?> _getNextDelay;

        public TestExecutionStrategy(
            DbContext context,
            int? retryCount = null,
            Func<Exception, bool> shouldRetryOn = null,
            Func<Exception, TimeSpan?> getNextDelay = null)
            : base(
                context,
                retryCount ?? DefaultMaxRetryCount,
                DefaultMaxDelay)
        {
            _shouldRetryOn = shouldRetryOn;
            _getNextDelay = getNextDelay;
        }

        protected TestExecutionStrategy()
            : base(
                (ExecutionStrategyDependencies)null,
                DefaultMaxRetryCount,
                DefaultMaxDelay)
        {
        }

        protected internal override bool ShouldRetryOn(Exception exception)
            => _shouldRetryOn?.Invoke(exception) == true;

        protected override TimeSpan? GetNextDelay(Exception lastException)
        {
            var baseDelay = base.GetNextDelay(lastException);
            return baseDelay != null && _getNextDelay != null ? _getNextDelay.Invoke(lastException) : baseDelay;
        }

        public TimeSpan? GetNextDelayBase(Exception lastException)
        {
            ExceptionsEncountered.Add(lastException);
            return base.GetNextDelay(lastException);
        }
    }
}
