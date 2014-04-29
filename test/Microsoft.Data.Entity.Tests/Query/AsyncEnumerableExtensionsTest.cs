// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Query;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Query
{
    public class AsyncEnumerableExtensionsTest
    {
        #region Any

        [Fact]
        public void AnyAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .AnyAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .AnyAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void AnyAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.AnyAsync(n => false, cancellationToken));
        }

        [Fact]
        public void AnyAsync_returns_result_if_found_before_cancellation_request()
        {
            var tokenSource = new CancellationTokenSource();
            var taskCancelled = false;

            var mockAsyncEnumerator = new Mock<IAsyncEnumerator<int>>();
            mockAsyncEnumerator
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken token) =>
                        {
                            Assert.False(taskCancelled);
                            tokenSource.Cancel();
                            taskCancelled = true;
                            return Task.FromResult(true);
                        });

            var mockAsyncEnumerable = new Mock<IAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Returns(mockAsyncEnumerator.Object);

            Assert.True(mockAsyncEnumerable.Object.AnyAsync(a => true, tokenSource.Token).GetAwaiter().GetResult());
        }

        #endregion

        #region Count

        [Fact]
        public void CountAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .CountAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .CountAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void CountAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.CountAsync(cancellationToken));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.CountAsync(n => true, cancellationToken));
        }

        #endregion

        #region First

        [Fact]
        public void FirstAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .FirstAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .FirstAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        #endregion

        #region ForEach

        [Fact]
        public void ForEachAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerator = new Mock<IAsyncEnumerator<int>>();
            mockAsyncEnumerator
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            var mockAsyncEnumerable = new Mock<IAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Returns(mockAsyncEnumerator.Object);

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .ForEachAsync(o => { }, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void Generic_ForEachAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.ForEachAsync(o => { }, cancellationToken));
        }

        #endregion

        #region Single

        [Fact]
        public void SingleAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .SingleAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .SingleAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void SingleAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.SingleAsync(cancellationToken));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.SingleAsync(n => true, cancellationToken));
        }

        [Fact]
        public void SingleAsync_checks_for_empty_sequence_before_checking_cancellation()
        {
            var tokenSource = new CancellationTokenSource();
            var taskCancelled = false;

            var mockAsyncEnumerator = new Mock<IAsyncEnumerator<object>>();
            mockAsyncEnumerator
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken token) =>
                        {
                            Assert.False(taskCancelled);
                            tokenSource.Cancel();
                            taskCancelled = true;
                            return Task.FromResult(false);
                        });

            var mockAsyncEnumerable = new Mock<IAsyncEnumerable<object>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Returns(mockAsyncEnumerator.Object);

            Assert.Equal(
                Strings.EmptySequence,
                Assert.Throws<InvalidOperationException>(
                    () => mockAsyncEnumerable.Object.SingleAsync(tokenSource.Token)
                        .GetAwaiter().GetResult()).Message);
        }

        #endregion

        #region SingleOrDefault

        [Fact]
        public void SingleOrDefaultAsync_throws_OperationCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerable = new Mock<IAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .SingleOrDefaultAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());

            Assert.Throws<OperationCanceledException>(
                () => mockAsyncEnumerable.Object
                    .SingleOrDefaultAsync(n => true, new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        [Fact]
        public void SingleOrDefaultAsync_checks_cancellation_token_when_enumerating_results()
        {
            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.SingleOrDefaultAsync(cancellationToken));

            AsyncMethod_checks_for_cancellation_when_enumerating_results<int>(
                (source, cancellationToken) => source.SingleOrDefaultAsync(n => true, cancellationToken));
        }

        #endregion

        #region ToArray

        [Fact]
        public void ToArrayAsync_throws_TaskCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerator = new Mock<IAsyncEnumerator<int>>();
            mockAsyncEnumerator
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            var mockAsyncEnumerable = new Mock<IAsyncEnumerable<int>>();

            Assert.Throws<TaskCanceledException>(
                () => mockAsyncEnumerable.Object
                    .ToArrayAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        #endregion

        #region ToList

        [Fact]
        public void ToListAsync_throws_TaskCanceledException_before_enumerating_if_task_is_cancelled()
        {
            var mockAsyncEnumerator = new Mock<IAsyncEnumerator<int>>();
            mockAsyncEnumerator
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("Not expected to be invoked - task has been cancelled."));

            var mockAsyncEnumerable = new Mock<IAsyncEnumerable<int>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Returns(mockAsyncEnumerator.Object);

            Assert.Throws<TaskCanceledException>(
                () => mockAsyncEnumerable.Object
                    .ToListAsync(new CancellationToken(canceled: true))
                    .GetAwaiter().GetResult());
        }

        #endregion

        private static void AsyncMethod_checks_for_cancellation_when_enumerating_results<T>(
            Func<IAsyncEnumerable<T>, CancellationToken, Task> asyncMethod)
        {
            var tokenSource = new CancellationTokenSource();
            var taskCancelled = false;

            var mockAsyncEnumerator = new Mock<IAsyncEnumerator<T>>();
            mockAsyncEnumerator
                .Setup(e => e.MoveNextAsync(It.IsAny<CancellationToken>()))
                .Returns(
                    (CancellationToken token) =>
                        {
                            Assert.False(taskCancelled);
                            tokenSource.Cancel();
                            taskCancelled = true;
                            return Task.FromResult(true);
                        });

            var mockAsyncEnumerable = new Mock<IAsyncEnumerable<T>>();
            mockAsyncEnumerable
                .Setup(e => e.GetAsyncEnumerator())
                .Returns(mockAsyncEnumerator.Object);

            Assert.Throws<OperationCanceledException>(
                () => asyncMethod(mockAsyncEnumerable.Object, tokenSource.Token)
                    .GetAwaiter().GetResult());
        }
    }
}
