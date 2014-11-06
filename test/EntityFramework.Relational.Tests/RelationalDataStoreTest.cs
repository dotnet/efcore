// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalDataStoreTest
    {
        [Fact]
        public async Task SaveChangesAsync_delegates()
        {
            var relationalConnectionMock = new Mock<RelationalConnection>();
            var commandBatchPreparerMock = new Mock<CommandBatchPreparer>();
            var batchExecutorMock = new Mock<BatchExecutor>();
            var relationalDataStore = (RelationalDataStore)new FakeRelationalDataStore(CreateDbContextConfiguration(),
                relationalConnectionMock.Object,
                commandBatchPreparerMock.Object,
                batchExecutorMock.Object,
                new LoggerFactory());

            var stateEntries = new List<StateEntry>();
            var cancellationToken = new CancellationTokenSource().Token;

            await relationalDataStore.SaveChangesAsync(stateEntries, cancellationToken);

            commandBatchPreparerMock.Verify(c => c.BatchCommands(stateEntries));
            batchExecutorMock.Verify(be => be.ExecuteAsync(It.IsAny<IEnumerable<ModificationCommandBatch>>(), relationalConnectionMock.Object, cancellationToken));
        }

        private DbContextConfiguration CreateDbContextConfiguration()
        {
            return new Mock<DbContextConfiguration>().Object;
        }

        private class FakeRelationalDataStore : RelationalDataStore
        {
            public FakeRelationalDataStore(
                DbContextConfiguration configuration,
                RelationalConnection connection,
                CommandBatchPreparer batchPreparer,
                BatchExecutor batchExecutor,
                ILoggerFactory loggerFactory)
                : base(configuration, connection, batchPreparer, batchExecutor, loggerFactory)
            {
            }
        }
    }
}
