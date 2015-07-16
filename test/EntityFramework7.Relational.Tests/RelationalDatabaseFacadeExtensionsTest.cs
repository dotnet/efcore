// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Framework.DependencyInjection;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests
{
    public class RelationalDatabaseFacadeExtensionsTest
    {
        [Fact]
        public void GetDbConnection_returns_the_current_connection()
        {
            var dbConnection = Mock.Of<DbConnection>();

            var connectionMock = new Mock<IRelationalConnection>();
            connectionMock.SetupGet(m => m.DbConnection).Returns(dbConnection);

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            Assert.Same(dbConnection, context.Database.GetDbConnection());
        }

        [Fact]
        public void GetDbConnection_returns_the_current_transaction()
        {
            var dbTransaction = Mock.Of<DbTransaction>();

            var connectionMock = new Mock<IRelationalConnection>();
            connectionMock.SetupGet(m => m.DbTransaction).Returns(dbTransaction);

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            Assert.Same(dbTransaction, context.Database.GetDbTransaction());
        }

        [Fact]
        public void Relational_specific_methods_throws_when_non_relational_provider_is_in_use()
        {
            var context = RelationalTestHelpers.Instance.CreateContext();

            Assert.Equal(
                Strings.RelationalNotInUse,
                Assert.Throws<InvalidOperationException>(() => context.Database.GetDbConnection()).Message);
        }

        [Fact]
        public void Can_open_the_underlying_connection()
        {
            var connectionMock = new Mock<IRelationalConnection>();

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            context.Database.OpenConnection();

            connectionMock.Verify(m => m.Open(), Times.Once);
        }

        [Fact]
        public void Can_open_the_underlying_connection_async()
        {
            var connectionMock = new Mock<IRelationalConnection>();

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            var cancellationToken = new CancellationToken();

            context.Database.OpenConnectionAsync(cancellationToken);

            connectionMock.Verify(m => m.OpenAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public void Can_close_the_underlying_connection()
        {
            var connectionMock = new Mock<IRelationalConnection>();

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            context.Database.CloseConnection();

            connectionMock.Verify(m => m.Close(), Times.Once);
        }

        [Fact]
        public void Can_begin_transaction()
        {
            var connectionMock = new Mock<IRelationalConnection>();
            var transaction = Mock.Of<IRelationalTransaction>();

            connectionMock.Setup(m => m.BeginTransaction()).Returns(transaction);

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            Assert.Same(transaction, context.Database.BeginTransaction());

            connectionMock.Verify(m => m.BeginTransaction(), Times.Once);
        }

        [Fact]
        public void Can_begin_transaction_with_isolation_level()
        {
            var connectionMock = new Mock<IRelationalConnection>();
            var transaction = Mock.Of<IRelationalTransaction>();

            connectionMock.Setup(m => m.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(transaction);

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            var isolationLevel = IsolationLevel.Chaos;

            Assert.Same(transaction, context.Database.BeginTransaction(isolationLevel));

            connectionMock.Verify(m => m.BeginTransaction(isolationLevel), Times.Once);
        }

        [Fact]
        public void Can_begin_transaction_async()
        {
            var connectionMock = new Mock<IRelationalConnection>();
            var transaction = Mock.Of<IRelationalTransaction>();

            var transactionTask = new Task<IRelationalTransaction>(() => transaction);

            connectionMock.Setup(m => m.BeginTransactionAsync(It.IsAny<CancellationToken>()))
                .Returns(transactionTask);

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            var cancellationToken = new CancellationToken();

            Assert.Same(transactionTask, context.Database.BeginTransactionAsync(cancellationToken));

            connectionMock.Verify(m => m.BeginTransactionAsync(cancellationToken), Times.Once);
        }

        [Fact]
        public void Can_begin_transaction_with_isolation_level_async()
        {
            var connectionMock = new Mock<IRelationalConnection>();
            var transaction = Mock.Of<IRelationalTransaction>();

            var transactionTask = new Task<IRelationalTransaction>(() => transaction);

            connectionMock.Setup(m => m.BeginTransactionAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
                .Returns(transactionTask);

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            var cancellationToken = new CancellationToken();
            var isolationLevel = IsolationLevel.Chaos;

            Assert.Same(transactionTask, context.Database.BeginTransactionAsync(isolationLevel, cancellationToken));

            connectionMock.Verify(m => m.BeginTransactionAsync(isolationLevel, cancellationToken), Times.Once);
        }

        [Fact]
        public void Can_use_transaction()
        {
            var connectionMock = new Mock<IRelationalConnection>();
            var dbTransaction = Mock.Of<DbTransaction>();
            var transaction = Mock.Of<IRelationalTransaction>();

            connectionMock.Setup(m => m.UseTransaction(dbTransaction)).Returns(transaction);

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            Assert.Same(transaction, context.Database.UseTransaction(dbTransaction));

            connectionMock.Verify(m => m.UseTransaction(dbTransaction), Times.Once);
        }

        [Fact]
        public void Can_commit_transaction()
        {
            var connectionMock = new Mock<IRelationalConnection>();
            var transactionMock = new Mock<IRelationalTransaction>();

            connectionMock.Setup(m => m.Transaction).Returns(transactionMock.Object);

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            context.Database.CommitTransaction();

            transactionMock.Verify(m => m.Commit(), Times.Once);
        }

        [Fact]
        public void Can_roll_back_transaction()
        {
            var connectionMock = new Mock<IRelationalConnection>();
            var transactionMock = new Mock<IRelationalTransaction>();

            connectionMock.Setup(m => m.Transaction).Returns(transactionMock.Object);

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(connectionMock.Object));

            context.Database.RollbackTransaction();

            transactionMock.Verify(m => m.Rollback(), Times.Once);
        }

        [Fact]
        public void Can_apply_migration()
        {
            var migratorMock = new Mock<IMigrator>();

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(migratorMock.Object));

            context.Database.ApplyMigrations();

            migratorMock.Verify(m => m.ApplyMigrations(null), Times.Once);
        }

        [Fact]
        public void Can_apply_migration_async()
        {
            var migratorMock = new Mock<IMigrator>();
            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddInstance(migratorMock.Object));

            context.Database.ApplyMigrationsAsync();

            migratorMock.Verify(m => m.ApplyMigrationsAsync(null), Times.Once);
        }
    }
}
