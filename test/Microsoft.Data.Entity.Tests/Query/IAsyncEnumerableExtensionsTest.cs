// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Query;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Query
{
    public class IAsyncEnumerableExtensionsTest
    {
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
