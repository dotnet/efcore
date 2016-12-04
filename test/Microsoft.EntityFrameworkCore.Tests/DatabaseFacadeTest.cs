// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests
{
    public class DatabaseFacadeTest
    {
        [Fact]
        public void Methods_delegate_to_configured_store_creator()
        {
            var creatorMock = new Mock<IDatabaseCreator>();
            creatorMock.Setup(m => m.EnsureCreated()).Returns(true);
            creatorMock.Setup(m => m.EnsureDeleted()).Returns(true);

            var context = TestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton(creatorMock.Object));

            Assert.True(context.Database.EnsureCreated());
            creatorMock.Verify(m => m.EnsureCreated(), Times.Once);

            Assert.True(context.Database.EnsureDeleted());
            creatorMock.Verify(m => m.EnsureDeleted(), Times.Once);
        }

        [Fact]
        public async void Async_methods_delegate_to_configured_store_creator()
        {
            var cancellationToken = new CancellationTokenSource().Token;

            var creatorMock = new Mock<IDatabaseCreator>();
            creatorMock.Setup(m => m.EnsureCreatedAsync(cancellationToken)).Returns(Task.FromResult(true));
            creatorMock.Setup(m => m.EnsureDeletedAsync(cancellationToken)).Returns(Task.FromResult(true));

            var context = TestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton(creatorMock.Object));

            Assert.True(await context.Database.EnsureCreatedAsync(cancellationToken));
            creatorMock.Verify(m => m.EnsureCreatedAsync(cancellationToken), Times.Once);

            Assert.True(await context.Database.EnsureDeletedAsync(cancellationToken));
            creatorMock.Verify(m => m.EnsureDeletedAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public void Can_get_IServiceProvider()
        {
            using (var context = TestHelpers.Instance.CreateContext())
            {
                Assert.Same(
                    ((IInfrastructure<IServiceProvider>)context).Instance,
                    ((IInfrastructure<IServiceProvider>)context.Database).Instance);
            }
        }

        [Fact]
        public void Can_get_DatabaseCreator()
        {
            using (var context = TestHelpers.Instance.CreateContext())
            {
                Assert.Same(
                    context.GetService<IDatabaseCreator>(),
                    context.Database.GetService<IDatabaseCreator>());
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

        [Fact]
        public void Can_begin_transaction()
        {
            var transactionManagerMock = new Mock<IDbContextTransactionManager>();
            var transaction = Mock.Of<IDbContextTransaction>();

            transactionManagerMock.Setup(m => m.BeginTransaction()).Returns(transaction);

            var context = TestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton(transactionManagerMock.Object));

            Assert.Same(transaction, context.Database.BeginTransaction());

            transactionManagerMock.Verify(m => m.BeginTransaction(), Times.Once);
        }

        [Fact]
        public void Can_begin_transaction_async()
        {
            var transactionManagerMock = new Mock<IDbContextTransactionManager>();
            var transaction = Mock.Of<IDbContextTransaction>();

            var transactionTask = new Task<IDbContextTransaction>(() => transaction);

            transactionManagerMock.Setup(m => m.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(transactionTask);

            var context = TestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton(transactionManagerMock.Object));

            var cancellationToken = new CancellationToken();

            Assert.Same(transactionTask, context.Database.BeginTransactionAsync(cancellationToken));

            transactionManagerMock.Verify(m => m.BeginTransactionAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public void Can_commit_transaction()
        {
            var transactionManagerMock = new Mock<IDbContextTransactionManager>();

            var context = TestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton(transactionManagerMock.Object));

            context.Database.CommitTransaction();

            transactionManagerMock.Verify(m => m.CommitTransaction(), Times.Once);
        }

        [Fact]
        public void Can_roll_back_transaction()
        {
            var transactionManagerMock = new Mock<IDbContextTransactionManager>();

            var context = TestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton(transactionManagerMock.Object));

            context.Database.RollbackTransaction();

            transactionManagerMock.Verify(m => m.RollbackTransaction(), Times.Once);
        }

        [Fact]
        public void Can_get_current_transaction()
        {
            var transactionManagerMock = new Mock<IDbContextTransactionManager>();
            var transaction = Mock.Of<IDbContextTransaction>();

            transactionManagerMock.Setup(m => m.CurrentTransaction).Returns(transaction);

            var context = TestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton(transactionManagerMock.Object));

            Assert.Same(transaction, context.Database.CurrentTransaction);
        }

        [Fact]
        public void Cannot_use_DatabaseFacade_after_dispose()
        {
            var context = TestHelpers.Instance.CreateContext();
            var facade = context.Database;
            context.Dispose();

            Assert.Throws<ObjectDisposedException>(() => context.Database.GetService<IModel>());

            foreach (var methodInfo in facade.GetType().GetMethods(BindingFlags.Public))
            {
                Assert.Throws<ObjectDisposedException>(() => methodInfo.Invoke(facade, null));
            }
        }
    }
}
