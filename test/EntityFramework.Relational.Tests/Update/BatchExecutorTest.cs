// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Update;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Update
{
    public class BatchExecutorTest
    {
        [Fact]
        public async Task ExecuteAsync_calls_Commit_if_no_transaction()
        {
            var sqlGenerator = new Mock<IUpdateSqlGenerator>().Object;
            var mockModificationCommandBatch = new Mock<ModificationCommandBatch>(sqlGenerator);
            mockModificationCommandBatch.Setup(m => m.ModificationCommands.Count).Returns(1);

            var mockRelationalConnection = new Mock<IRelationalConnection>();
            var transactionMock = new Mock<IRelationalTransaction>();

            IRelationalTransaction currentTransaction = null;
            mockRelationalConnection.Setup(m => m.BeginTransaction()).Returns(() => currentTransaction = transactionMock.Object);
            mockRelationalConnection.Setup(m => m.Transaction).Returns(() => currentTransaction);

            var cancellationToken = new CancellationTokenSource().Token;

            var relationalTypeMapper = new ConcreteTypeMapper();
            var batchExecutor = new BatchExecutorForTest(relationalTypeMapper);

            await batchExecutor.ExecuteAsync(new[] { mockModificationCommandBatch.Object }, mockRelationalConnection.Object, cancellationToken);

            mockRelationalConnection.Verify(rc => rc.OpenAsync(cancellationToken));
            mockRelationalConnection.Verify(rc => rc.Close());
            transactionMock.Verify(t => t.Commit());
            mockModificationCommandBatch.Verify(mcb => mcb.ExecuteAsync(
                It.IsAny<IRelationalTransaction>(),
                relationalTypeMapper,
                It.IsAny<DbContext>(),
                null,
                cancellationToken));
        }

        [Fact]
        public async Task ExecuteAsync_does_not_call_Commit_if_existing_transaction()
        {
            var sqlGenerator = new Mock<IUpdateSqlGenerator>().Object;
            var mockModificationCommandBatch = new Mock<ModificationCommandBatch>(sqlGenerator);
            mockModificationCommandBatch.Setup(m => m.ModificationCommands.Count).Returns(1);

            var mockRelationalConnection = new Mock<IRelationalConnection>();
            var transactionMock = new Mock<IRelationalTransaction>();
            mockRelationalConnection.Setup(m => m.Transaction).Returns(transactionMock.Object);

            var cancellationToken = new CancellationTokenSource().Token;

            var relationalTypeMapper = new ConcreteTypeMapper();
            var batchExecutor = new BatchExecutorForTest(relationalTypeMapper);

            await batchExecutor.ExecuteAsync(new[] { mockModificationCommandBatch.Object }, mockRelationalConnection.Object, cancellationToken);

            mockRelationalConnection.Verify(rc => rc.OpenAsync(cancellationToken));
            mockRelationalConnection.Verify(rc => rc.Close());
            mockRelationalConnection.Verify(rc => rc.BeginTransaction(), Times.Never);
            transactionMock.Verify(t => t.Commit(), Times.Never);
            mockModificationCommandBatch.Verify(mcb => mcb.ExecuteAsync(
                It.IsAny<IRelationalTransaction>(),
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

            protected override ILogger Logger => null;
        }

        private class ConcreteTypeMapper : RelationalTypeMapper
        {
            protected override string GetColumnType(IProperty property) => property.TestProvider().ColumnType;

            protected override IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings { get; }
                = new Dictionary<Type, RelationalTypeMapping>();

            protected override IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings { get; }
                = new Dictionary<string, RelationalTypeMapping>();
        }
    }
}
