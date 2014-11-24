// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.Tests
{
    public class RedisDatabaseTests
    {
        [Fact]
        public void Delegates_to_datastore_creator()
        {
            var model = Mock.Of<IModel>();
            var connection = Mock.Of<RedisConnection>();
            var configurationMock = new Mock<ContextServices>();
            configurationMock.Setup(m => m.Model).Returns(model);
            configurationMock.Setup(m => m.Connection).Returns(connection);

            var creatorMock = new Mock<RedisDataStoreCreator>(configurationMock.Object);
            creatorMock.Setup(m => m.EnsureCreated(model)).Returns(true);
            creatorMock.Setup(m => m.EnsureDeleted(model)).Returns(true);
            configurationMock.Setup(m => m.DataStoreCreator).Returns(creatorMock.Object);

            var database = new RedisDatabase(
                new ContextService<IModel>(() => model),
                creatorMock.Object,
                connection,
                new LoggerFactory());

            Assert.True(database.EnsureCreated());
            creatorMock.Verify(m => m.EnsureCreated(model), Times.Once);

            Assert.True(database.EnsureDeleted());
            creatorMock.Verify(m => m.EnsureDeleted(model), Times.Once);

            Assert.Same(connection, database.Connection);
        }

        // TODO: add some tests for other methods in RedisDatabase
    }
}
