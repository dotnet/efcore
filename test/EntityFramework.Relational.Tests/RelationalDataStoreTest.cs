// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        public void Constructor_check_arguments()
        {
            Assert.Equal(
                "configuration",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    (RelationalDataStore)new FakeRelationalDataStore(null,
                        new Mock<RelationalConnection>().Object,
                        new Mock<CommandBatchPreparer>().Object,
                        new Mock<BatchExecutor>().Object)).ParamName);

            Assert.Equal(
                "connection",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    (RelationalDataStore)new FakeRelationalDataStore(CreateDbContextConfiguration(),
                        null,
                        new Mock<CommandBatchPreparer>().Object,
                        new Mock<BatchExecutor>().Object)).ParamName);

            Assert.Equal(
                "batchPreparer",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    (RelationalDataStore)new FakeRelationalDataStore(CreateDbContextConfiguration(),
                        new Mock<RelationalConnection>().Object,
                        null,
                        new Mock<BatchExecutor>().Object)).ParamName);

            Assert.Equal(
                "batchExecutor",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    (RelationalDataStore)new FakeRelationalDataStore(CreateDbContextConfiguration(),
                        new Mock<RelationalConnection>().Object,
                        new Mock<CommandBatchPreparer>().Object,
                        null)).ParamName);
        }

        [Fact]
        public void SaveChangesAsync_checks_arguments()
        {
            var relationalDataStore = (RelationalDataStore)new FakeRelationalDataStore(CreateDbContextConfiguration(),
                new Mock<RelationalConnection>().Object,
                new Mock<CommandBatchPreparer>().Object,
                new Mock<BatchExecutor>().Object);

            Assert.Equal(
                "stateEntries",
                // ReSharper disable once AssignNullToNotNullAttribute
                Assert.Throws<ArgumentNullException>(() =>
                    relationalDataStore.SaveChangesAsync(null).Wait()).ParamName);
        }

        [Fact]
        public async Task SaveChangesAsync_delegates()
        {
            var relationalConnectionMock = new Mock<RelationalConnection>();
            var commandBatchPreparerMock = new Mock<CommandBatchPreparer>();
            var batchExecutorMock = new Mock<BatchExecutor>();
            var relationalDataStore = (RelationalDataStore)new FakeRelationalDataStore(CreateDbContextConfiguration(),
                relationalConnectionMock.Object,
                commandBatchPreparerMock.Object,
                batchExecutorMock.Object);

            var stateEntries = new List<StateEntry>();
            var cancellationToken = new CancellationTokenSource().Token;

            await relationalDataStore.SaveChangesAsync(stateEntries, cancellationToken);

            commandBatchPreparerMock.Verify(c => c.BatchCommands(stateEntries));
            batchExecutorMock.Verify(be => be.ExecuteAsync(It.IsAny<IEnumerable<ModificationCommandBatch>>(), relationalConnectionMock.Object, cancellationToken));
        }

        private DbContextConfiguration CreateDbContextConfiguration()
        {
            var mockDbContextConfiguration = new Mock<DbContextConfiguration>();

            mockDbContextConfiguration.Setup(m => m.LoggerFactory).Returns(new Mock<ILoggerFactory>().Object);

            return mockDbContextConfiguration.Object;
        }

        private class FakeRelationalDataStore : RelationalDataStore
        {
            public FakeRelationalDataStore(
                DbContextConfiguration configuration,
                RelationalConnection connection,
                CommandBatchPreparer batchPreparer,
                BatchExecutor batchExecutor)
                : base(configuration, connection, batchPreparer, batchExecutor)
            {
            }
        }
    }
}
