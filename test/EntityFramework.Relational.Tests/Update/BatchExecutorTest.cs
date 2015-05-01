// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Update
{
    public class BatchExecutorTest
    {
        [Fact]
        public async Task ExecuteAsync_calls_Commit_if_no_transaction()
        {
            var sqlGenerator = new Mock<ISqlGenerator>().Object;
            var mockModificationCommandBatch = new Mock<ModificationCommandBatch>(sqlGenerator);

            var mockRelationalConnection = new Mock<IRelationalConnection>();
            var transactionMock = new Mock<RelationalTransaction>(
                mockRelationalConnection.Object, Mock.Of<DbTransaction>(), false, Mock.Of<ILogger>());

            RelationalTransaction currentTransaction = null;
            mockRelationalConnection.Setup(m => m.BeginTransaction()).Returns(() => currentTransaction = transactionMock.Object);
            mockRelationalConnection.Setup(m => m.Transaction).Returns(() => currentTransaction);

            var cancellationToken = new CancellationTokenSource().Token;

            var relationalTypeMapper = new RelationalTypeMapper();
            var batchExecutor = new BatchExecutorForTest(relationalTypeMapper);

            await batchExecutor.ExecuteAsync(new[] { mockModificationCommandBatch.Object }, mockRelationalConnection.Object, cancellationToken);

            mockRelationalConnection.Verify(rc => rc.OpenAsync(cancellationToken));
            mockRelationalConnection.Verify(rc => rc.Close());
            transactionMock.Verify(t => t.Commit());
            mockModificationCommandBatch.Verify(mcb => mcb.ExecuteAsync(
                It.IsAny<RelationalTransaction>(),
                relationalTypeMapper,
                It.IsAny<DbContext>(),
                null,
                cancellationToken));
        }

        [Fact]
        public async Task ExecuteAsync_does_not_call_Commit_if_existing_transaction()
        {
            var sqlGenerator = new Mock<ISqlGenerator>().Object;
            var mockModificationCommandBatch = new Mock<ModificationCommandBatch>(sqlGenerator);

            var mockRelationalConnection = new Mock<IRelationalConnection>();
            var transactionMock = new Mock<RelationalTransaction>(
                mockRelationalConnection.Object, Mock.Of<DbTransaction>(), false, Mock.Of<ILogger>());
            mockRelationalConnection.Setup(m => m.Transaction).Returns(transactionMock.Object);

            var cancellationToken = new CancellationTokenSource().Token;

            var relationalTypeMapper = new RelationalTypeMapper();
            var batchExecutor = new BatchExecutorForTest(relationalTypeMapper);

            await batchExecutor.ExecuteAsync(new[] { mockModificationCommandBatch.Object }, mockRelationalConnection.Object, cancellationToken);

            mockRelationalConnection.Verify(rc => rc.OpenAsync(cancellationToken));
            mockRelationalConnection.Verify(rc => rc.Close());
            mockRelationalConnection.Verify(rc => rc.BeginTransaction(), Times.Never);
            transactionMock.Verify(t => t.Commit(), Times.Never);
            mockModificationCommandBatch.Verify(mcb => mcb.ExecuteAsync(
                It.IsAny<RelationalTransaction>(),
                relationalTypeMapper,
                It.IsAny<DbContext>(),
                null,
                cancellationToken));
        }

        private class BatchExecutorForTest : BatchExecutor
        {
            public BatchExecutorForTest(IRelationalTypeMapper typeMapper)
                : base(typeMapper, TestHelpers.Instance.CreateContext(), new LoggerFactory())
            {
            }

            protected override ILogger Logger
            {
                get { return null; }
            }
        }
    }
}
