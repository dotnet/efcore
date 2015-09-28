// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Data.Entity.Update.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Update
{
    public class BatchExecutorTest
    {
        [Fact]
        public async Task ExecuteAsync_calls_Commit_if_no_transaction()
        {
            var mockModificationCommandBatch = new Mock<ModificationCommandBatch>();
            mockModificationCommandBatch.Setup(m => m.ModificationCommands.Count).Returns(1);

            var mockRelationalConnection = new Mock<IRelationalConnection>();
            var transactionMock = new Mock<IRelationalTransaction>();

            IRelationalTransaction currentTransaction = null;
            mockRelationalConnection.Setup(m => m.BeginTransaction()).Returns(() => currentTransaction = transactionMock.Object);
            mockRelationalConnection.Setup(m => m.Transaction).Returns(() => currentTransaction);

            var cancellationToken = new CancellationTokenSource().Token;

            var batchExecutor = new BatchExecutorForTest();

            await batchExecutor.ExecuteAsync(new[] { mockModificationCommandBatch.Object }, mockRelationalConnection.Object, cancellationToken);

            mockRelationalConnection.Verify(rc => rc.OpenAsync(cancellationToken));
            mockRelationalConnection.Verify(rc => rc.Close());
            transactionMock.Verify(t => t.Commit());

            mockModificationCommandBatch.Verify(mcb => mcb.ExecuteAsync(
                It.IsAny<IRelationalConnection>(),
                cancellationToken));
        }

        [Fact]
        public async Task ExecuteAsync_does_not_call_Commit_if_existing_transaction()
        {
            var mockModificationCommandBatch = new Mock<ModificationCommandBatch>();
            mockModificationCommandBatch.Setup(m => m.ModificationCommands.Count).Returns(1);

            var mockRelationalConnection = new Mock<IRelationalConnection>();
            var transactionMock = new Mock<IRelationalTransaction>();
            mockRelationalConnection.Setup(m => m.Transaction).Returns(transactionMock.Object);

            var cancellationToken = new CancellationTokenSource().Token;

            var batchExecutor = new BatchExecutorForTest();

            await batchExecutor.ExecuteAsync(new[] { mockModificationCommandBatch.Object }, mockRelationalConnection.Object, cancellationToken);

            mockRelationalConnection.Verify(rc => rc.OpenAsync(cancellationToken));
            mockRelationalConnection.Verify(rc => rc.Close());
            mockRelationalConnection.Verify(rc => rc.BeginTransaction(), Times.Never);
            transactionMock.Verify(t => t.Commit(), Times.Never);
            mockModificationCommandBatch.Verify(mcb => mcb.ExecuteAsync(
                It.IsAny<IRelationalConnection>(),
                cancellationToken));
        }

        private class BatchExecutorForTest : BatchExecutor
        {
            public BatchExecutorForTest()
                : base(new Mock<ISensitiveDataLogger<BatchExecutor>>().Object)
            {
            }
        }
    }
}
