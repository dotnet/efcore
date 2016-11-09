// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class ExecutionStrategyTests
    {
        public class TestExecutionStrategy : ExecutionStrategy
        {
            public TestExecutionStrategy(DbContext context)
                : base(new ExecutionStrategyContext(context, null), DefaultMaxRetryCount, DefaultMaxDelay)
            {
            }

            public TestExecutionStrategy(DbContext context, int retryCount)
                : base(new ExecutionStrategyContext(context, null), retryCount, DefaultMaxDelay)
            {
            }

            protected internal override bool ShouldRetryOn(Exception exception) => false;

            public new virtual TimeSpan? GetNextDelay(Exception lastException)
            {
                ExceptionsEncountered.Add(lastException);
                return base.GetNextDelay(lastException);
            }

            public new static bool Suspended
            {
                get { return ExecutionStrategy.Suspended; }
                set { ExecutionStrategy.Suspended = value; }
            }
        }

        [Fact]
        public void GetNextDelay_returns_the_expected_default_sequence()
        {
            var strategy = new TestExecutionStrategy(CreateContext());
            var delays = new List<TimeSpan>();
            var delay = strategy.GetNextDelay(new Exception());
            while (delay != null)
            {
                delays.Add(delay.Value);
                delay = strategy.GetNextDelay(new Exception());
            }

            var expectedDelays = new List<TimeSpan>
            {
                TimeSpan.FromSeconds(0),
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(3),
                TimeSpan.FromSeconds(7),
                TimeSpan.FromSeconds(15)
            };

            Assert.Equal(expectedDelays.Count, delays.Count);
            for (var i = 0; i < expectedDelays.Count; i++)
            {
                Assert.True(
                    Math.Abs((delays[i] - expectedDelays[i]).TotalMilliseconds) <=
                    expectedDelays[i].TotalMilliseconds * 0.1 + 1,
                    string.Format("Expected: {0}; Actual: {1}", expectedDelays[i], delays[i]));
            }
        }

        [Fact]
        public void RetriesOnFailure_returns_true()
        {
            var mockExecutionStrategy = new TestExecutionStrategy(CreateContext());

            Assert.True(mockExecutionStrategy.RetriesOnFailure);
        }

        [Fact]
        public void Execute_Action_throws_for_an_existing_transaction()
        {
            Execute_throws_for_an_existing_transaction(e => e.Execute(() => { }));
        }

        [Fact]
        public void Execute_Func_throws_for_an_existing_transaction()
        {
            Execute_throws_for_an_existing_transaction(e => e.Execute(() => 1));
        }

        private void Execute_throws_for_an_existing_transaction(Action<ExecutionStrategy> executeAsync)
        {
            var context = CreateContext();
            var mockExecutionStrategy = new TestExecutionStrategy(context);
            using (context.Database.BeginTransaction())
            {
                Assert.Equal(
                    CoreStrings.ExecutionStrategyExistingTransaction(mockExecutionStrategy.GetType().Name, "DbContext.Database.CreateExecutionStrategy()"),
                    Assert.Throws<InvalidOperationException>(
                        () =>
                            executeAsync(mockExecutionStrategy)).Message);
            }
        }

        [Fact]
        public void Execute_Action_does_not_throw_when_invoked_twice()
        {
            Execute_does_not_throw_when_invoked_twice((e, f) => e.Execute(() => { f(); }));
        }

        [Fact]
        public void Execute_Func_does_not_throw_when_invoked_twice()
        {
            Execute_does_not_throw_when_invoked_twice((e, f) => e.Execute(f));
        }

        private void Execute_does_not_throw_when_invoked_twice(Action<ExecutionStrategy, Func<int>> execute)
        {
            var executed = false;

            var executionStrategyMock = new Mock<TestExecutionStrategy>(CreateContext())
            {
                CallBase = true
            };

            executionStrategyMock.Setup(m => m.ShouldRetryOn(It.IsAny<Exception>())).Returns<Exception>(
                e => e is ArgumentOutOfRangeException);

            for (var i = 0; i < 2; i++)
            {
                execute(
                    executionStrategyMock.Object, () =>
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

        [Fact]
        public void Execute_Action_doesnt_retry_if_succesful()
        {
            Execute_doesnt_retry_if_succesful((e, f) => e.Execute(() => { f(); }));
        }

        [Fact]
        public void Execute_Func_doesnt_retry_if_succesful()
        {
            Execute_doesnt_retry_if_succesful((e, f) => e.Execute(f));
        }

        private void Execute_doesnt_retry_if_succesful(Action<ExecutionStrategy, Func<int>> execute)
        {
            var executionStrategyMock = new Mock<TestExecutionStrategy>(CreateContext())
            {
                CallBase = true
            };

            executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                e =>
                    {
                        Assert.True(false);
                        return null;
                    });
            executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                e =>
                    {
                        Assert.True(false);
                        return false;
                    });

            var executionCount = 0;
            execute(executionStrategyMock.Object, () => executionCount++);

            Assert.Equal(1, executionCount);
        }

        [Fact]
        public void Execute_Action_doesnt_retry_if_suspended()
        {
            Execute_doesnt_retry_if_suspended((e, f) => e.Execute(() => { f(); }));
        }

        [Fact]
        public void Execute_Func_doesnt_retry_if_suspended()
        {
            Execute_doesnt_retry_if_suspended((e, f) => e.Execute(f));
        }

        private void Execute_doesnt_retry_if_suspended(Action<ExecutionStrategy, Func<int>> execute)
        {
            var executionStrategyMock = new Mock<TestExecutionStrategy>(CreateContext())
            {
                CallBase = true
            };

            executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                e =>
                    {
                        Assert.True(false);
                        return null;
                    });
            executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                e =>
                    {
                        Assert.True(false);
                        return true;
                    });

            TestExecutionStrategy.Suspended = true;
            var executionCount = 0;
            Assert.Throws<DbUpdateException>(() =>
                execute(executionStrategyMock.Object, () =>
                    {
                        executionCount++;
                        throw new DbUpdateException("", new ArgumentOutOfRangeException());
                    }));
            TestExecutionStrategy.Suspended = false;

            Assert.Equal(1, executionCount);
        }

        [Fact]
        public void Execute_Action_retries_until_succesful()
        {
            Execute_retries_until_succesful((e, f) => e.Execute(() => { f(); }));
        }

        [Fact]
        public void Execute_Func_retries_until_succesful()
        {
            Execute_retries_until_succesful((e, f) => e.Execute(f));
        }

        private void Execute_retries_until_succesful(Action<ExecutionStrategy, Func<int>> execute)
        {
            var executionStrategyMock = new Mock<TestExecutionStrategy>(CreateContext())
            {
                CallBase = true
            };

            executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                e => TimeSpan.FromTicks(0));
            executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                e => e is ArgumentOutOfRangeException);

            var executionCount = 0;

            execute(
                executionStrategyMock.Object, () =>
                    {
                        if (executionCount++ < 3)
                        {
                            throw new DbUpdateException("", new ArgumentOutOfRangeException());
                        }

                        return executionCount;
                    });

            Assert.Equal(4, executionCount);
        }

        [Fact]
        public void Execute_Action_retries_until_not_retrieable_exception_is_thrown()
        {
            Execute_retries_until_not_retrieable_exception_is_thrown((e, f) => e.Execute(() => { f(); }));
        }

        [Fact]
        public void Execute_Func_retries_until_not_retrieable_exception_is_thrown()
        {
            Execute_retries_until_not_retrieable_exception_is_thrown((e, f) => e.Execute(f));
        }

        private void Execute_retries_until_not_retrieable_exception_is_thrown(Action<ExecutionStrategy, Func<int>> execute)
        {
            var executionStrategyMock = new Mock<TestExecutionStrategy>(CreateContext())
            {
                    CallBase = true
                };

            executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                e => TimeSpan.FromTicks(0));
            executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                e => e is ArgumentOutOfRangeException);

            var executionCount = 0;

            Assert.Throws<ArgumentNullException>(
                () =>
                    execute(
                        executionStrategyMock.Object, () =>
                            {
                                if (executionCount++ < 3)
                                {
                                    throw new ArgumentOutOfRangeException();
                                }
                                throw new ArgumentNullException();
                            }));

            Assert.Equal(4, executionCount);
        }

        [Fact]
        public void Execute_Action_retries_until_limit_is_reached()
        {
            Execute_retries_until_limit_is_reached((e, f) => e.Execute(() => { f(); }));
        }

        [Fact]
        public void Execute_Func_retries_until_limit_is_reached()
        {
            Execute_retries_until_limit_is_reached((e, f) => e.Execute(f));
        }

        private void Execute_retries_until_limit_is_reached(Action<ExecutionStrategy, Func<int>> execute)
        {
            var executionCount = 0;

            var executionStrategyMock = new Mock<TestExecutionStrategy>(CreateContext(), 2)
            {
                CallBase = true
            };

            executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                e => e is ArgumentOutOfRangeException);

            Assert.IsType<ArgumentOutOfRangeException>(
                Assert.Throws<RetryLimitExceededException>(
                    () =>
                        execute(
                            executionStrategyMock.Object, () =>
                                {
                                    if (executionCount++ < 3)
                                    {
                                        throw new ArgumentOutOfRangeException();
                                    }
                                    Assert.True(false);
                                    return 0;
                                })).InnerException);

            Assert.Equal(3, executionCount);
        }

        [Fact]
        public Task ExecuteAsync_Action_throws_for_an_existing_transaction()
        {
            return ExecuteAsync_throws_for_an_existing_transaction(e => e.ExecuteAsync(() => (Task)Task.FromResult(1)));
        }

        [Fact]
        public Task ExecuteAsync_Func_throws_for_an_existing_transaction()
        {
            return ExecuteAsync_throws_for_an_existing_transaction(e => e.ExecuteAsync((s, ct) => Task.FromResult(1), null, CancellationToken.None));
        }

        private async Task ExecuteAsync_throws_for_an_existing_transaction(Func<ExecutionStrategy, Task> executeAsync)
        {
            var context = CreateContext();
            var mockExecutionStrategy = new TestExecutionStrategy(context);
            using (context.Database.BeginTransaction())
            {
                Assert.Equal(
                    CoreStrings.ExecutionStrategyExistingTransaction(mockExecutionStrategy.GetType().Name, "DbContext.Database.CreateExecutionStrategy()"),
                    (await Assert.ThrowsAsync<InvalidOperationException>(
                        () =>
                            executeAsync(mockExecutionStrategy))).Message);
            }
        }

        [Fact]
        public Task ExecuteAsync_Action_does_not_throw_when_invoked_twice()
        {
            return ExecuteAsync_does_not_throw_when_invoked_twice((e, f) => e.ExecuteAsync(() => (Task)f(CancellationToken.None)));
        }

        [Fact]
        public Task ExecuteAsync_Func_does_not_throw_when_invoked_twice()
        {
            return ExecuteAsync_does_not_throw_when_invoked_twice((e, f) => e.ExecuteAsync(f, CancellationToken.None));
        }

        private async Task ExecuteAsync_does_not_throw_when_invoked_twice(Func<ExecutionStrategy, Func<CancellationToken, Task<int>>, Task> executeAsync)
        {
            var executed = false;
            var executionStrategyMock = new Mock<TestExecutionStrategy>(CreateContext())
            {
                CallBase = true
            };

            executionStrategyMock.Setup(m => m.ShouldRetryOn(It.IsAny<Exception>())).Returns<Exception>(
                e => e is ArgumentOutOfRangeException);

            for (var i = 0; i < 2; i++)
            {
                await executeAsync(
                    executionStrategyMock.Object, ct =>
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

        [Fact]
        public Task ExecuteAsync_Action_doesnt_retry_if_succesful()
        {
            return ExecuteAsync_doesnt_retry_if_succesful((e, f) => e.ExecuteAsync(ct => (Task)f(ct), CancellationToken.None));
        }

        [Fact]
        public Task ExecuteAsync_Func_doesnt_retry_if_succesful()
        {
            return ExecuteAsync_doesnt_retry_if_succesful((e, f) => e.ExecuteAsync(f, CancellationToken.None));
        }

        private async Task ExecuteAsync_doesnt_retry_if_succesful(Func<ExecutionStrategy, Func<CancellationToken, Task<int>>, Task> executeAsync)
        {
            var executionStrategyMock = new Mock<TestExecutionStrategy>(CreateContext())
            {
                CallBase = true
            };

            executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                e =>
                    {
                        Assert.True(false);
                        return null;
                    });
            executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                e =>
                    {
                        Assert.True(false);
                        return false;
                    });

            var executionCount = 0;
            await executeAsync(executionStrategyMock.Object, ct => Task.FromResult(executionCount++));

            Assert.Equal(1, executionCount);
        }

        [Fact]
        public Task ExecuteAsync_Action_doesnt_retry_if_suspended()
        {
            return ExecuteAsync_doesnt_retry_if_suspended((e, f) => e.ExecuteAsync(() => (Task)f(CancellationToken.None)));
        }

        [Fact]
        public Task ExecuteAsync_Func_doesnt_retry_if_suspended()
        {
            return ExecuteAsync_doesnt_retry_if_suspended((e, f) => e.ExecuteAsync(f, CancellationToken.None));
        }

        private async Task ExecuteAsync_doesnt_retry_if_suspended(Func<ExecutionStrategy, Func<CancellationToken, Task<int>>, Task> executeAsync)
        {
            var executionStrategyMock = new Mock<TestExecutionStrategy>(CreateContext())
                {
                    CallBase = true
                };

            executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                e =>
                    {
                        Assert.True(false);
                        return null;
                    });
            executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                e =>
                    {
                        Assert.True(false);
                        return true;
                    });

            TestExecutionStrategy.Suspended = true;
            var executionCount = 0;
            await Assert.ThrowsAsync<DbUpdateException>(
                () =>
                    executeAsync(
                        executionStrategyMock.Object, ct =>
                            {
                                executionCount++;
                                throw new DbUpdateException("", new ArgumentOutOfRangeException());
                            }));
            TestExecutionStrategy.Suspended = false;

            Assert.Equal(1, executionCount);
        }

        [Fact]
        public Task ExecuteAsync_Action_retries_until_succesful()
        {
            return ExecuteAsync_retries_until_succesful((e, f) => e.ExecuteAsync(ct => (Task)f(ct), CancellationToken.None));
        }

        [Fact]
        public Task ExecuteAsync_Func_retries_until_succesful()
        {
            return ExecuteAsync_retries_until_succesful((e, f) => e.ExecuteAsync(f, CancellationToken.None));
        }

        private async Task ExecuteAsync_retries_until_succesful(Func<ExecutionStrategy, Func<CancellationToken, Task<int>>, Task> executeAsync)
        {
            var executionStrategyMock = new Mock<TestExecutionStrategy>(CreateContext())
            {
                CallBase = true
            };

            executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                e => TimeSpan.FromTicks(0));
            executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                e => e is ArgumentOutOfRangeException);

            var executionCount = 0;

            await executeAsync(
                executionStrategyMock.Object, ct =>
                    {
                        if (executionCount++ < 3)
                        {
                            throw new DbUpdateException("", new ArgumentOutOfRangeException());
                        }

                        return Task.FromResult(executionCount);
                    });

            Assert.Equal(4, executionCount);
        }

        [Fact]
        public Task ExecuteAsync_Action_retries_until_not_retrieable_exception_is_thrown()
        {
            return ExecuteAsync_retries_until_not_retrieable_exception_is_thrown(
                (e, f) => e.ExecuteAsync(ct => (Task)f(ct), CancellationToken.None));
        }

        [Fact]
        public Task ExecuteAsync_Func_retries_until_not_retrieable_exception_is_thrown()
        {
            return ExecuteAsync_retries_until_not_retrieable_exception_is_thrown((e, f) => e.ExecuteAsync(f, CancellationToken.None));
        }

        private async Task ExecuteAsync_retries_until_not_retrieable_exception_is_thrown(
            Func<ExecutionStrategy, Func<CancellationToken, Task<int>>, Task> executeAsync)
        {
            var executionStrategyMock = new Mock<TestExecutionStrategy>(CreateContext())
            {
                CallBase = true
            };

            executionStrategyMock.Setup(m => m.GetNextDelay(It.IsAny<Exception>())).Returns<Exception>(
                e => TimeSpan.FromTicks(0));
            executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                e => e is ArgumentOutOfRangeException);

            var executionCount = 0;

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => executeAsync(
                    executionStrategyMock.Object, ct =>
                        {
                            if (executionCount++ < 3)
                            {
                                throw new ArgumentOutOfRangeException();
                            }
                            throw new ArgumentNullException();
                        }));

            Assert.Equal(4, executionCount);
        }

        [Fact]
        public Task ExecuteAsync_Action_retries_until_limit_is_reached()
        {
            return ExecuteAsync_retries_until_limit_is_reached((e, f) => e.ExecuteAsync(ct => (Task)f(ct), CancellationToken.None));
        }

        [Fact]
        public Task ExecuteAsync_Func_retries_until_limit_is_reached()
        {
            return ExecuteAsync_retries_until_limit_is_reached((e, f) => e.ExecuteAsync(f, CancellationToken.None));
        }

        private async Task ExecuteAsync_retries_until_limit_is_reached(Func<ExecutionStrategy, Func<CancellationToken, Task<int>>, Task> executeAsync)
        {
            var executionCount = 0;

            var executionStrategyMock = new Mock<TestExecutionStrategy>(CreateContext(), 2)
            {
                CallBase = true
            };

            executionStrategyMock.Protected().Setup<bool>("ShouldRetryOn", ItExpr.IsAny<Exception>()).Returns<Exception>(
                e => e is ArgumentOutOfRangeException);

            Assert.IsType<ArgumentOutOfRangeException>(
                (await Assert.ThrowsAsync<RetryLimitExceededException>(
                    () =>
                        executeAsync(
                            executionStrategyMock.Object, ct =>
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

        protected DbContext CreateContext()
            => TestHelpers.Instance.CreateContext(
                TestHelpers.Instance.CreateServiceProvider(
                    new ServiceCollection()
                        .AddScoped<InMemoryTransactionManager, TestInMemoryTransactionManager>()),
                TestHelpers.Instance.CreateOptions());
    }
}
