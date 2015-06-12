// Copyright (c) .NET Foundation. All rights reserved.
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
            var creatorMock = new Mock<RelationalDataStoreCreator>(model);
            creatorMock.Setup(m => m.Exists()).Returns(true);
            creatorMock.Setup(m => m.HasTables()).Returns(true);
            creatorMock.Setup(m => m.EnsureCreated()).Returns(true);
            creatorMock.Setup(m => m.EnsureDeleted()).Returns(true);

            var connectionMock = new Mock<IRelationalConnection>();
            var dbConnectionMock = new Mock<DbConnection>();
            connectionMock.SetupGet(m => m.DbConnection).Returns(dbConnectionMock.Object);
            dbConnectionMock.SetupGet(m => m.Database).Returns("MyDb");

            var database = new ConcreteRelationalDatabase(
                context,
                creatorMock.Object,
                connectionMock.Object,
                Mock.Of<IMigrator>(),
                new LoggerFactory());

            Assert.True(database.Exists());
            creatorMock.Verify(m => m.Exists(), Times.Once);

            database.Create();
            creatorMock.Verify(m => m.Create(), Times.Once);

            database.CreateTables();
            creatorMock.Verify(m => m.CreateTables(), Times.Once);

            Assert.True(database.HasTables());
            creatorMock.Verify(m => m.HasTables(), Times.Once);

            database.Delete();
            creatorMock.Verify(m => m.Delete(), Times.Once);

            Assert.True(database.EnsureCreated());
            creatorMock.Verify(m => m.EnsureCreated(), Times.Once);

            Assert.True(database.EnsureDeleted());
            creatorMock.Verify(m => m.EnsureDeleted(), Times.Once);

            Assert.Same(connectionMock.Object, database.Connection);
        }

        [Fact]
        public async void Async_methods_delegate_to_configured_store_creator()
        {
            var context = TestHelpers.Instance.CreateContext();
            var model = context.Model;
            var cancellationToken = new CancellationTokenSource().Token;

            var creatorMock = new Mock<RelationalDataStoreCreator>(model);
            creatorMock.Setup(m => m.ExistsAsync(cancellationToken)).Returns(Task.FromResult(true));
            creatorMock.Setup(m => m.HasTablesAsync(cancellationToken)).Returns(Task.FromResult(true));
            creatorMock.Setup(m => m.EnsureCreatedAsync(cancellationToken)).Returns(Task.FromResult(true));
            creatorMock.Setup(m => m.EnsureDeletedAsync(cancellationToken)).Returns(Task.FromResult(true));

            var connectionMock = new Mock<IRelationalConnection>();
            var dbConnectionMock = new Mock<DbConnection>();
            connectionMock.SetupGet(m => m.DbConnection).Returns(dbConnectionMock.Object);
            dbConnectionMock.SetupGet(m => m.Database).Returns("MyDb");

            var database = new ConcreteRelationalDatabase(
                context,
                creatorMock.Object,
                connectionMock.Object,
                Mock.Of<IMigrator>(),
                new LoggerFactory());

            Assert.True(await database.ExistsAsync(cancellationToken));
            creatorMock.Verify(m => m.ExistsAsync(cancellationToken), Times.Once);

            await database.CreateAsync(cancellationToken);
            creatorMock.Verify(m => m.CreateAsync(cancellationToken), Times.Once);

            await database.CreateTablesAsync(cancellationToken);
            creatorMock.Verify(m => m.CreateTablesAsync(cancellationToken), Times.Once);

            Assert.True(await database.HasTablesAsync(cancellationToken));
            creatorMock.Verify(m => m.HasTablesAsync(cancellationToken), Times.Once);

            await database.DeleteAsync(cancellationToken);
            creatorMock.Verify(m => m.DeleteAsync(cancellationToken), Times.Once);

            Assert.True(await database.EnsureCreatedAsync(cancellationToken));
            creatorMock.Verify(m => m.EnsureCreatedAsync(cancellationToken), Times.Once);

            Assert.True(await database.EnsureDeletedAsync(cancellationToken));
            creatorMock.Verify(m => m.EnsureDeletedAsync(cancellationToken), Times.Once);
        }

        private class ConcreteRelationalDatabase : RelationalDatabase
        {
            public ConcreteRelationalDatabase(
                DbContext context,
                RelationalDataStoreCreator dataStoreCreator,
                IRelationalConnection connection,
                IMigrator migrator,
                ILoggerFactory loggerFactory)
                : base(context, dataStoreCreator, connection, migrator, loggerFactory)
            {
            }
        }
    }
}
