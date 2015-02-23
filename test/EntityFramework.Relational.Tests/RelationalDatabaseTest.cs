// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Tests;
using Microsoft.Framework.Logging;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalDatabaseTest
    {
        [Fact]
        public void Methods_delegate_to_configured_store_creator()
        {
            var context = TestHelpers.Instance.CreateContext();
            var model = context.Model;
            var creatorMock = new Mock<RelationalDataStoreCreator>();
            creatorMock.Setup(m => m.Exists()).Returns(true);
            creatorMock.Setup(m => m.HasTables()).Returns(true);
            creatorMock.Setup(m => m.EnsureCreated(model)).Returns(true);
            creatorMock.Setup(m => m.EnsureDeleted(model)).Returns(true);

            var connectionMock = new Mock<RelationalConnection>();
            var dbConnectionMock = new Mock<DbConnection>();
            connectionMock.SetupGet(m => m.DbConnection).Returns(dbConnectionMock.Object);
            dbConnectionMock.SetupGet(m => m.Database).Returns("MyDb");

            var database = new ConcreteRelationalDatabase(
                context,
                creatorMock.Object,
                connectionMock.Object,
                Mock.Of<Migrator>(),
                new LoggerFactory());

            Assert.True(database.Exists());
            creatorMock.Verify(m => m.Exists(), Times.Once);

            database.Create();
            creatorMock.Verify(m => m.Create(), Times.Once);

            database.CreateTables();
            creatorMock.Verify(m => m.CreateTables(model), Times.Once);

            Assert.True(database.HasTables());
            creatorMock.Verify(m => m.HasTables(), Times.Once);

            database.Delete();
            creatorMock.Verify(m => m.Delete(), Times.Once);

            Assert.True(database.EnsureCreated());
            creatorMock.Verify(m => m.EnsureCreated(model), Times.Once);

            Assert.True(database.EnsureDeleted());
            creatorMock.Verify(m => m.EnsureDeleted(model), Times.Once);

            Assert.Same(connectionMock.Object, database.Connection);
        }

        [Fact]
        public async void Async_methods_delegate_to_configured_store_creator()
        {
            var context = TestHelpers.Instance.CreateContext();
            var model = context.Model;
            var cancellationToken = new CancellationTokenSource().Token;

            var creatorMock = new Mock<RelationalDataStoreCreator>();
            creatorMock.Setup(m => m.ExistsAsync(cancellationToken)).Returns(Task.FromResult(true));
            creatorMock.Setup(m => m.HasTablesAsync(cancellationToken)).Returns(Task.FromResult(true));
            creatorMock.Setup(m => m.EnsureCreatedAsync(model, cancellationToken)).Returns(Task.FromResult(true));
            creatorMock.Setup(m => m.EnsureDeletedAsync(model, cancellationToken)).Returns(Task.FromResult(true));

            var connectionMock = new Mock<RelationalConnection>();
            var dbConnectionMock = new Mock<DbConnection>();
            connectionMock.SetupGet(m => m.DbConnection).Returns(dbConnectionMock.Object);
            dbConnectionMock.SetupGet(m => m.Database).Returns("MyDb");

            var database = new ConcreteRelationalDatabase(
                context,
                creatorMock.Object,
                connectionMock.Object,
                Mock.Of<Migrator>(),
                new LoggerFactory());

            Assert.True(await database.ExistsAsync(cancellationToken));
            creatorMock.Verify(m => m.ExistsAsync(cancellationToken), Times.Once);

            await database.CreateAsync(cancellationToken);
            creatorMock.Verify(m => m.CreateAsync(cancellationToken), Times.Once);

            await database.CreateTablesAsync(cancellationToken);
            creatorMock.Verify(m => m.CreateTablesAsync(model, cancellationToken), Times.Once);

            Assert.True(await database.HasTablesAsync(cancellationToken));
            creatorMock.Verify(m => m.HasTablesAsync(cancellationToken), Times.Once);

            await database.DeleteAsync(cancellationToken);
            creatorMock.Verify(m => m.DeleteAsync(cancellationToken), Times.Once);

            Assert.True(await database.EnsureCreatedAsync(cancellationToken));
            creatorMock.Verify(m => m.EnsureCreatedAsync(model, cancellationToken), Times.Once);

            Assert.True(await database.EnsureDeletedAsync(cancellationToken));
            creatorMock.Verify(m => m.EnsureDeletedAsync(model, cancellationToken), Times.Once);
        }

        private class ConcreteRelationalDatabase : RelationalDatabase
        {
            public ConcreteRelationalDatabase(
                DbContext context,
                RelationalDataStoreCreator dataStoreCreator,
                RelationalConnection connection,
                Migrator migrator,
                ILoggerFactory loggerFactory)
                : base(context, dataStoreCreator, connection, migrator, loggerFactory)
            {
            }
        }
    }
}
