// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class AtsDatabaseTests
    {
        [Fact]
        public void Delegates_to_datastore_creator()
        {
            var model = Mock.Of<IModel>();
            var connection = Mock.Of<AtsConnection>();
            var creatorMock = new Mock<AtsDataStoreCreator>(connection);
            creatorMock.Setup(m => m.EnsureCreated(model)).Returns(true);
            creatorMock.Setup(m => m.EnsureDeleted(model)).Returns(true);

            var configurationMock = new Mock<DbContextConfiguration>();
            configurationMock.Setup(m => m.DataStoreCreator).Returns(creatorMock.Object);
            configurationMock.Setup(m => m.Model).Returns(model);
            configurationMock.Setup(m => m.Connection).Returns(connection);

            var database = new AtsDatabase(configurationMock.Object, new LoggerFactory());

            Assert.True(database.EnsureCreated());
            creatorMock.Verify(m => m.EnsureCreated(model), Times.Once);

            Assert.True(database.EnsureDeleted());
            creatorMock.Verify(m => m.EnsureDeleted(model), Times.Once);

            Assert.Same(connection, database.Connection);
        }
    }
}
