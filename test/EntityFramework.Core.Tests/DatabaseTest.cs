// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class DatabaseTest
    {
        [Fact]
        public void Methods_delegate_to_configured_store_creator()
        {
            var context = TestHelpers.Instance.CreateContext();

            var creatorMock = new Mock<IDataStoreCreator>();
            creatorMock.Setup(m => m.EnsureCreated()).Returns(true);
            creatorMock.Setup(m => m.EnsureDeleted()).Returns(true);

            var database = new Database(
                context,
                creatorMock.Object);

            Assert.True(database.EnsureCreated());
            creatorMock.Verify(m => m.EnsureCreated(), Times.Once);

            Assert.True(database.EnsureDeleted());
            creatorMock.Verify(m => m.EnsureDeleted(), Times.Once);
        }

        [Fact]
        public async void Async_methods_delegate_to_configured_store_creator()
        {
            var context = TestHelpers.Instance.CreateContext();
            var cancellationToken = new CancellationTokenSource().Token;

            var creatorMock = new Mock<IDataStoreCreator>();
            creatorMock.Setup(m => m.EnsureCreatedAsync(cancellationToken)).Returns(Task.FromResult(true));
            creatorMock.Setup(m => m.EnsureDeletedAsync(cancellationToken)).Returns(Task.FromResult(true));

            var database = new Database(
                context,
                creatorMock.Object);

            Assert.True(await database.EnsureCreatedAsync(cancellationToken));
            creatorMock.Verify(m => m.EnsureCreatedAsync(cancellationToken), Times.Once);

            Assert.True(await database.EnsureDeletedAsync(cancellationToken));
            creatorMock.Verify(m => m.EnsureDeletedAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public void Can_get_IServiceProvider()
        {
            using (var context = TestHelpers.Instance.CreateContext())
            {
                Assert.Same(
                    ((IAccessor<IServiceProvider>)context).Service,
                    ((IAccessor<IServiceProvider>)context.Database).Service);
            }
        }

        [Fact]
        public void Can_get_DataStoreCreator()
        {
            using (var context = TestHelpers.Instance.CreateContext())
            {
                Assert.Same(
                    context.GetService<IDataStoreCreator>(),
                    context.Database.GetService<IDataStoreCreator>());
            }
        }

        [Fact]
        public void Can_get_Model()
        {
            using (var context = TestHelpers.Instance.CreateContext())
            {
                Assert.Same(context.GetService<IModel>(), context.Database.GetService<IModel>());
            }
        }
    }
}
