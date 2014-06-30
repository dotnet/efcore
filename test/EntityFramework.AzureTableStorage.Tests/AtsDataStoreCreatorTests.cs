// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.AzureTableStorage.Requests;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class AtsDataStoreCreatorTests
    {
        private readonly Mock<AtsConnection> _connection;
        private readonly AtsDataStoreCreator _creator;
        private readonly Metadata.Model _model;

        public AtsDataStoreCreatorTests()
        {
            _connection = new Mock<AtsConnection>();
            _connection.Setup(
               s => s.ExecuteRequest(
                   It.IsAny<TableRequest<bool>>(),
                   It.IsAny<ILogger>())
               ).Returns(true);
            _connection.Setup(
                s => s.ExecuteRequestAsync(
                    It.IsAny<TableRequest<bool>>(),
                    It.IsAny<ILogger>(),
                    It.IsAny<CancellationToken>())
                ).Returns(Task.FromResult(true));

            _creator = new AtsDataStoreCreator(_connection.Object);
            _model = new Metadata.Model();
            var builder = new ModelBuilder(_model);
            builder.Entity("Test1");
            builder.Entity("Test2");
            builder.Entity("Test3");
        }

        [Fact]
        public void Ensure_creation()
        {
            Assert.True(_creator.EnsureCreated(_model));
            _connection.Verify(m => m.ExecuteRequest(
                It.IsAny<CreateTableRequest>(),
                It.IsAny<ILogger>()),
                Times.Exactly(3));
        }

        [Fact]
        public void Ensures_creation_async()
        {
            Assert.True(_creator.EnsureCreatedAsync(_model).Result);
            _connection.Verify(m => m.ExecuteRequestAsync(
                It.IsAny<CreateTableRequest>(),
                It.IsAny<ILogger>(),
                It.IsAny<CancellationToken>()),
                Times.Exactly(3));
        }

        [Fact]
        public void Ensures_deletion()
        {
            Assert.True(_creator.EnsureDeleted(_model));
            _connection.Verify(m => m.ExecuteRequest(
                It.IsAny<DeleteTableRequest>(),
                It.IsAny<ILogger>()),
                Times.Exactly(3));
            ;
        }

        [Fact]
        public void Ensures_deletion_async()
        {
            Assert.True(_creator.EnsureDeletedAsync(_model).Result);
            _connection.Verify(m => m.ExecuteRequestAsync(
                It.IsAny<DeleteTableRequest>(),
                It.IsAny<ILogger>(),
                It.IsAny<CancellationToken>()),
                Times.Exactly(3));
            ;
            ;
        }
    }
}
