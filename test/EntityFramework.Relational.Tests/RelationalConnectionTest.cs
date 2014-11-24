// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests
{
    public class RelationalConnectionTest
    {
        [Fact]
        public void Can_create_new_connection_lazily_using_given_connection_string()
        {
            using (var connection = new FakeConnection(CreateOptions(e => e.ConnectionString = "Database=FrodoLives")))
            {
                Assert.Equal(0, connection.CreateCount);

                var dbConnection = connection.DbConnection;

                Assert.Equal(1, connection.CreateCount);
                Assert.Equal("Database=FrodoLives", dbConnection.ConnectionString);
            }
        }

        [Fact]
        public void Lazy_connection_is_opened_and_closed_when_necessary()
        {
            using (var connection = new FakeConnection(CreateOptions(e => e.ConnectionString = "Database=FrodoLives")))
            {
                Assert.Equal(0, connection.CreateCount);

                connection.Open();

                Assert.Equal(1, connection.CreateCount);

                var connectionMock = Mock.Get(connection.DbConnection);
                connectionMock.Verify(m => m.Open(), Times.Once);

                connection.Open();
                connection.Open();

                connectionMock.Verify(m => m.Open(), Times.Once);

                connection.Close();
                connection.Close();

                connectionMock.Verify(m => m.Open(), Times.Once);
                connectionMock.Verify(m => m.Close(), Times.Never);

                connection.Close();

                connectionMock.Verify(m => m.Open(), Times.Once);
                connectionMock.Verify(m => m.Close(), Times.Once);

                connection.Open();

                connectionMock.Verify(m => m.Open(), Times.Exactly(2));

                connection.Close();

                connectionMock.Verify(m => m.Open(), Times.Exactly(2));
                connectionMock.Verify(m => m.Close(), Times.Exactly(2));
            }
        }

        [Fact]
        public async Task Lazy_connection_is_async_opened_and_closed_when_necessary()
        {
            using (var connection = new FakeConnection(CreateOptions(e => e.ConnectionString = "Database=FrodoLives")))
            {
                Assert.Equal(0, connection.CreateCount);

                var cancellationToken = new CancellationTokenSource().Token;
                await connection.OpenAsync(cancellationToken);

                Assert.Equal(1, connection.CreateCount);

                var connectionMock = Mock.Get(connection.DbConnection);
                connectionMock.Verify(m => m.OpenAsync(cancellationToken), Times.Once);

                await connection.OpenAsync(cancellationToken);
                await connection.OpenAsync(cancellationToken);

                connectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Once);

                connection.Close();
                connection.Close();

                connectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Once);
                connectionMock.Verify(m => m.Close(), Times.Never);

                connection.Close();

                connectionMock.Verify(m => m.OpenAsync(It.IsAny<CancellationToken>()), Times.Once);
                connectionMock.Verify(m => m.Close(), Times.Once);

                await connection.OpenAsync(cancellationToken);

                connectionMock.Verify(m => m.OpenAsync(cancellationToken), Times.Exactly(2));

                connection.Close();

                connectionMock.Verify(m => m.OpenAsync(cancellationToken), Times.Exactly(2));
                connectionMock.Verify(m => m.Close(), Times.Exactly(2));
            }
        }

        [Fact]
        public void Lazy_connection_is_recreated_if_used_again_after_being_disposed()
        {
            var connection = new FakeConnection(CreateOptions(e => e.ConnectionString = "Database=FrodoLives"));

            Assert.Equal(0, connection.CreateCount);
            var connectionMock = Mock.Get(connection.DbConnection);
            Assert.Equal(1, connection.CreateCount);

            connection.Open();
            connection.Close();

            connection.Dispose();

            connectionMock.Verify(m => m.Open(), Times.Once);
            connectionMock.Verify(m => m.Close(), Times.Once);
            connectionMock.Protected().Verify("Dispose", Times.Once(), new object[] { true });

            Assert.Equal(1, connection.CreateCount);
            connectionMock = Mock.Get(connection.DbConnection);
            Assert.Equal(2, connection.CreateCount);

            connection.Open();
            connection.Close();

            connection.Dispose();

            connectionMock.Verify(m => m.Open(), Times.Once);
            connectionMock.Verify(m => m.Close(), Times.Once);
            connectionMock.Protected().Verify("Dispose", Times.Once(), new object[] { true });
        }

        [Fact]
        public void Lazy_connection_is_not_created_just_so_it_can_be_disposed()
        {
            var connection = new FakeConnection(CreateOptions(e => e.ConnectionString = "Database=FrodoLives"));

            connection.Dispose();

            Assert.Equal(0, connection.CreateCount);
        }

        [Fact]
        public void Can_create_new_connection_from_exsting_DbConnection()
        {
            var dbConnection = CreateDbConnectionMock("Database=FrodoLives").Object;

            using (var connection = new FakeConnection(CreateOptions(e => e.Connection = dbConnection)))
            {
                Assert.Equal(0, connection.CreateCount);

                Assert.Same(dbConnection, connection.DbConnection);

                Assert.Equal(0, connection.CreateCount);
            }
        }

        [Fact]
        public void Existing_connection_is_opened_and_closed_when_necessary()
        {
            var connectionMock = CreateDbConnectionMock("Database=FrodoLives");
            connectionMock.Setup(m => m.State).Returns(ConnectionState.Closed);

            using (var connection = new FakeConnection(CreateOptions(e => e.Connection = connectionMock.Object)))
            {
                Assert.Equal(0, connection.CreateCount);

                connection.Open();

                Assert.Equal(0, connection.CreateCount);

                connectionMock.Verify(m => m.Open(), Times.Once);

                connection.Open();
                connection.Open();

                connectionMock.Verify(m => m.Open(), Times.Once);

                connection.Close();
                connection.Close();

                connectionMock.Verify(m => m.Open(), Times.Once);
                connectionMock.Verify(m => m.Close(), Times.Never);

                connection.Close();

                connectionMock.Verify(m => m.Open(), Times.Once);
                connectionMock.Verify(m => m.Close(), Times.Once);

                connection.Open();

                connectionMock.Verify(m => m.Open(), Times.Exactly(2));

                connection.Close();

                connectionMock.Verify(m => m.Open(), Times.Exactly(2));
                connectionMock.Verify(m => m.Close(), Times.Exactly(2));
            }
        }

        [Fact]
        public void Existing_connection_can_start_in_opened_state()
        {
            var connectionMock = CreateDbConnectionMock("Database=FrodoLives");
            connectionMock.Setup(m => m.State).Returns(ConnectionState.Open);

            using (var connection = new FakeConnection(CreateOptions(e => e.Connection = connectionMock.Object)))
            {
                Assert.Equal(0, connection.CreateCount);

                connection.Open();

                Assert.Equal(0, connection.CreateCount);

                connectionMock.Verify(m => m.Open(), Times.Never);

                connection.Open();
                connection.Open();

                connectionMock.Verify(m => m.Open(), Times.Never);

                connection.Close();
                connection.Close();

                connectionMock.Verify(m => m.Open(), Times.Never);
                connectionMock.Verify(m => m.Close(), Times.Never);

                connection.Close();

                connectionMock.Verify(m => m.Open(), Times.Never);
                connectionMock.Verify(m => m.Close(), Times.Never);

                connection.Open();

                connectionMock.Verify(m => m.Open(), Times.Never);

                connection.Close();

                connectionMock.Verify(m => m.Open(), Times.Never);
                connectionMock.Verify(m => m.Close(), Times.Never);
            }
        }

        [Fact]
        public void Existing_connection_is_not_disposed_even_after_being_opened_and_closed()
        {
            var connectionMock = CreateDbConnectionMock("Database=FrodoLives");
            var connection = new FakeConnection(CreateOptions(e => e.Connection = connectionMock.Object));

            Assert.Equal(0, connection.CreateCount);
            Assert.Same(connectionMock.Object, connection.DbConnection);

            connection.Open();
            connection.Close();
            connection.Dispose();

            connectionMock.Verify(m => m.Open(), Times.Once);
            connectionMock.Verify(m => m.Close(), Times.Once);
            connectionMock.Protected().Verify("Dispose", Times.Never(), new object[] { true });

            Assert.Equal(0, connection.CreateCount);
            Assert.Same(connectionMock.Object, connection.DbConnection);

            connection.Open();
            connection.Close();
            connection.Dispose();

            connectionMock.Verify(m => m.Open(), Times.Exactly(2));
            connectionMock.Verify(m => m.Close(), Times.Exactly(2));
            connectionMock.Protected().Verify("Dispose", Times.Never(), new object[] { true });
        }

        [Fact]
        public void Throws_if_no_relational_store_configured()
        {
            Assert.Equal(
                Strings.NoDataStoreConfigured,
                Assert.Throws<InvalidOperationException>(() => new FakeConnection(CreateOptions(null))).Message);
        }

        [Fact]
        public void Throws_if_multiple_relational_stores_configured()
        {
            Assert.Equal(
                Strings.MultipleDataStoresConfigured,
                Assert.Throws<InvalidOperationException>(() => new FakeConnection(CreateOptions(e => { }, e => { }))).Message);
        }

        [Fact]
        public void Throws_if_no_connection_or_connection_string_is_specified()
        {
            Assert.Equal(
                Strings.NoConnectionOrConnectionString,
                Assert.Throws<InvalidOperationException>(() => new FakeConnection(CreateOptions(e => { }))).Message);
        }

        [Fact]
        public void Throws_if_both_connection_and_connection_string_are_specified()
        {
            Assert.Equal(
                Strings.ConnectionAndConnectionString,
                Assert.Throws<InvalidOperationException>(() => new FakeConnection(
                    CreateOptions(
                        e =>
                            {
                                e.Connection = CreateDbConnectionMock("Database=FrodoLives").Object;
                                e.ConnectionString = "Database=FrodoLives";
                            }))).Message);
        }

        private static DbContextService<IDbContextOptions> CreateOptions(
            Action<FakeOptionsExtension1> configUpdater1,
            Action<FakeOptionsExtension2> configUpdater2 = null)
        {
            IDbContextOptions contextOptions = new DbContextOptions();

            if (configUpdater1 != null)
            {
                contextOptions.AddOrUpdateExtension(configUpdater1);
            }

            if (configUpdater2 != null)
            {
                contextOptions.AddOrUpdateExtension(configUpdater2);
            }

            return new DbContextService<IDbContextOptions>(contextOptions);
        }

        private class FakeConnection : RelationalConnection
        {
            public FakeConnection(DbContextService<IDbContextOptions> options)
                : base(options, new LoggerFactory())
            {
            }

            public int CreateCount { get; set; }

            protected override DbConnection CreateDbConnection()
            {
                CreateCount++;
                return CreateDbConnectionMock(ConnectionString).Object;
            }
        }

        private static Mock<DbConnection> CreateDbConnectionMock(string connectionString)
        {
            var connectionMock = new Mock<DbConnection>();
            connectionMock.Setup(m => m.ConnectionString).Returns(connectionString);
            return connectionMock;
        }

        private class FakeOptionsExtension1 : RelationalOptionsExtension
        {
            protected override void ApplyServices(EntityServicesBuilder builder)
            {
            }
        }

        private class FakeOptionsExtension2 : RelationalOptionsExtension
        {
            protected override void ApplyServices(EntityServicesBuilder builder)
            {
            }
        }
    }
}
