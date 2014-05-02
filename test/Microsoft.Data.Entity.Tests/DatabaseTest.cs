// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DatabaseTest
    {
        [Fact]
        public void Methods_delegate_to_configured_store_creator()
        {
            var creatorMock = new Mock<DataStoreCreator>();
            creatorMock.Setup(m => m.Exists()).Returns(true);

            var model = Mock.Of<IModel>();
            var connection = Mock.Of<DataStoreConnection>();
            var configurationMock = new Mock<ContextConfiguration>();
            configurationMock.Setup(m => m.DataStoreCreator).Returns(creatorMock.Object);
            configurationMock.Setup(m => m.Model).Returns(model);
            configurationMock.Setup(m => m.Connection).Returns(connection);

            var database = new Database(configurationMock.Object);

            Assert.True(database.Exists());
            creatorMock.Verify(m => m.Exists(), Times.Once);

            database.Create();
            creatorMock.Verify(m => m.Create(model), Times.Once);

            database.Delete();
            creatorMock.Verify(m => m.Delete(), Times.Once);

            Assert.Same(connection, database.Connection);
        }

        [Fact]
        public async void Async_methods_delegate_to_configured_store_creator()
        {
            var cancellationToken = new CancellationTokenSource().Token;

            var creatorMock = new Mock<DataStoreCreator>();
            creatorMock.Setup(m => m.ExistsAsync(cancellationToken)).Returns(Task.FromResult(true));

            var model = Mock.Of<IModel>();
            var configurationMock = new Mock<ContextConfiguration>();
            configurationMock.Setup(m => m.DataStoreCreator).Returns(creatorMock.Object);
            configurationMock.Setup(m => m.Model).Returns(model);

            var database = new Database(configurationMock.Object);

            Assert.True(await database.ExistsAsync(cancellationToken));
            creatorMock.Verify(m => m.ExistsAsync(cancellationToken), Times.Once);

            await database.CreateAsync(cancellationToken);
            creatorMock.Verify(m => m.CreateAsync(model, cancellationToken), Times.Once);

            await database.DeleteAsync(cancellationToken);
            creatorMock.Verify(m => m.DeleteAsync(cancellationToken), Times.Once);
        }
    }
}
