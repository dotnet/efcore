// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Update
{
    public class BatchExecutorTest
    {
        [Fact]
        public async Task ExecuteAsync_calls_Commit_if_no_transaction()
        {
            var mockModificationCommandBatch = new Mock<ModificationCommandBatch>();
            mockModificationCommandBatch.Setup(m => m.ModificationCommands.Count).Returns(1);

            var mockRelationalConnection = new Mock<IRelationalConnection>();
            var transactionMock = new Mock<IDbContextTransaction>();

            IDbContextTransaction currentTransaction = null;
            mockRelationalConnection.Setup(m => m.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(() => Task.FromResult(currentTransaction = transactionMock.Object));
            mockRelationalConnection.Setup(m => m.CurrentTransaction).Returns(() => currentTransaction);

            var cancellationToken = new CancellationTokenSource().Token;

            var batchExecutor = new BatchExecutor(
                new CurrentDbContext(new DbContext(new DbContextOptionsBuilder().Options)), new ExecutionStrategyFactory());

            await batchExecutor.ExecuteAsync(new[] { mockModificationCommandBatch.Object }, mockRelationalConnection.Object, cancellationToken);

            mockRelationalConnection.Verify(rc => rc.BeginTransactionAsync(cancellationToken));
            mockRelationalConnection.Verify(rc => rc.Close(), Times.Never);
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
            var transactionMock = new Mock<IDbContextTransaction>();
            mockRelationalConnection.Setup(m => m.CurrentTransaction).Returns(transactionMock.Object);

            var cancellationToken = new CancellationTokenSource().Token;

            var batchExecutor = new BatchExecutor(
                new CurrentDbContext(new DbContext(new DbContextOptionsBuilder().Options)), new ExecutionStrategyFactory());

            await batchExecutor.ExecuteAsync(new[] { mockModificationCommandBatch.Object }, mockRelationalConnection.Object, cancellationToken);

            mockRelationalConnection.Verify(rc => rc.OpenAsync(cancellationToken));
            mockRelationalConnection.Verify(rc => rc.Close());
            mockRelationalConnection.Verify(rc => rc.BeginTransaction(), Times.Never);
            transactionMock.Verify(t => t.Commit(), Times.Never);
            mockModificationCommandBatch.Verify(mcb => mcb.ExecuteAsync(
                It.IsAny<IRelationalConnection>(),
                cancellationToken));
        }
    }
}
