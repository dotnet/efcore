// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
            var model = context.Model;

            var creatorMock = new Mock<DataStoreCreator>();
            creatorMock.Setup(m => m.EnsureCreated(model)).Returns(true);
            creatorMock.Setup(m => m.EnsureDeleted(model)).Returns(true);

            var connection = Mock.Of<DataStoreConnection>();

            var database = new ConcreteDatabase(
                context,
                creatorMock.Object,
                connection,
                new LoggerFactory());

            Assert.True(database.EnsureCreated());
            creatorMock.Verify(m => m.EnsureCreated(model), Times.Once);

            Assert.True(database.EnsureDeleted());
            creatorMock.Verify(m => m.EnsureDeleted(model), Times.Once);

            Assert.Same(connection, database.Connection);
        }

        [Fact]
        public async void Async_methods_delegate_to_configured_store_creator()
        {
            var context = TestHelpers.Instance.CreateContext();
            var model = context.Model;
            var cancellationToken = new CancellationTokenSource().Token;

            var creatorMock = new Mock<DataStoreCreator>();
            creatorMock.Setup(m => m.EnsureCreatedAsync(model, cancellationToken)).Returns(Task.FromResult(true));
            creatorMock.Setup(m => m.EnsureDeletedAsync(model, cancellationToken)).Returns(Task.FromResult(true));

            var database = new ConcreteDatabase(
                context,
                creatorMock.Object,
                Mock.Of<DataStoreConnection>(),
                new LoggerFactory());

            Assert.True(await database.EnsureCreatedAsync(cancellationToken));
            creatorMock.Verify(m => m.EnsureCreatedAsync(model, cancellationToken), Times.Once);

            Assert.True(await database.EnsureDeletedAsync(cancellationToken));
            creatorMock.Verify(m => m.EnsureDeletedAsync(model, cancellationToken), Times.Once);
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
                    ((IAccessor<IServiceProvider>)context).Service.GetRequiredService<DataStoreCreator>(),
                    ((IAccessor<DataStoreCreator>)context.Database).Service);
            }
        }

        [Fact]
        public void Can_get_Model()
        {
            using (var context = TestHelpers.Instance.CreateContext())
            {
                Assert.Same(
                    ((IAccessor<IServiceProvider>)context).Service.GetRequiredService<IModel>(),
                    ((IAccessor<IModel>)context.Database).Service);
            }
        }

        [Fact]
        public void Can_get_Logger()
        {
            using (var context = TestHelpers.Instance.CreateContext())
            {
                Assert.NotNull(((IAccessor<ILogger>)context.Database).Service);
            }
        }

        private class ConcreteDatabase : Database
        {
            public ConcreteDatabase(
                DbContext context,
                DataStoreCreator dataStoreCreator,
                DataStoreConnection connection,
                ILoggerFactory loggerFactory)
                : base(context, dataStoreCreator, connection, loggerFactory)
            {
            }
        }
    }
}
