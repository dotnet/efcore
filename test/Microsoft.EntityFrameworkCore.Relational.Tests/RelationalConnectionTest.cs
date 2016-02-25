// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.FunctionalTests.TestUtilities;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests
{
    public class RelationalConnectionTest
    {
        [Fact]
        public void Can_create_new_connection_lazily_using_given_connection_string()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                var dbConnection = connection.DbConnection;

                Assert.Equal(1, connection.DbConnections.Count);
                Assert.Equal("Database=FrodoLives", dbConnection.ConnectionString);
            }
        }

        [Fact]
        public void Lazy_connection_is_opened_and_closed_when_necessary()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                connection.Open();

                Assert.Equal(1, connection.DbConnections.Count);

                var dbConnection = connection.DbConnections[0];
                Assert.Equal(1, dbConnection.OpenCount);

                connection.Open();
                connection.Open();

                Assert.Equal(1, dbConnection.OpenCount);

                connection.Close();
                connection.Close();

                Assert.Equal(1, dbConnection.OpenCount);
                Assert.Equal(0, dbConnection.CloseCount);

                connection.Close();

                Assert.Equal(1, dbConnection.OpenCount);
                Assert.Equal(1, dbConnection.CloseCount);

                connection.Open();

                Assert.Equal(2, dbConnection.OpenCount);

                connection.Close();

                Assert.Equal(2, dbConnection.OpenCount);
                Assert.Equal(2, dbConnection.CloseCount);
            }
        }

        [Fact]
        public async Task Lazy_connection_is_async_opened_and_closed_when_necessary()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                var cancellationToken = new CancellationTokenSource().Token;
                await connection.OpenAsync(cancellationToken);

                Assert.Equal(1, connection.DbConnections.Count);

                var dbConnection = connection.DbConnections[0];
                Assert.Equal(1, dbConnection.OpenAsyncCount);

                await connection.OpenAsync(cancellationToken);
                await connection.OpenAsync(cancellationToken);

                Assert.Equal(1, dbConnection.OpenAsyncCount);

                connection.Close();
                connection.Close();

                Assert.Equal(1, dbConnection.OpenAsyncCount);
                Assert.Equal(0, dbConnection.CloseCount);

                connection.Close();

                Assert.Equal(1, dbConnection.OpenAsyncCount);
                Assert.Equal(1, dbConnection.CloseCount);

                await connection.OpenAsync(cancellationToken);

                Assert.Equal(2, dbConnection.OpenAsyncCount);

                connection.Close();

                Assert.Equal(2, dbConnection.OpenAsyncCount);
                Assert.Equal(2, dbConnection.CloseCount);
            }
        }

        [Fact]
        public void Lazy_connection_is_recreated_if_used_again_after_being_disposed()
        {
            var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" }));

            Assert.Equal(0, connection.DbConnections.Count);
            var dbConnection = (FakeDbConnection)connection.DbConnection;
            Assert.Equal(1, connection.DbConnections.Count);

            connection.Open();

#if NET451
            // On CoreCLR, DbConnection.Dispose() calls DbConnection.Close()
            connection.Close();
#endif

            connection.Dispose();

            Assert.Equal(1, dbConnection.OpenCount);
            Assert.Equal(1, dbConnection.CloseCount);
            Assert.Equal(1, dbConnection.DisposeCount);

            Assert.Equal(1, connection.DbConnections.Count);
            dbConnection = (FakeDbConnection)connection.DbConnection;
            Assert.Equal(2, connection.DbConnections.Count);

            connection.Open();

#if NET451
            connection.Close();
#endif

            connection.Dispose();

            Assert.Equal(1, dbConnection.OpenCount);
            Assert.Equal(1, dbConnection.CloseCount);
            Assert.Equal(1, dbConnection.DisposeCount);
        }

        [Fact]
        public void Lazy_connection_is_not_created_just_so_it_can_be_disposed()
        {
            var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" }));

            connection.Dispose();

            Assert.Equal(0, connection.DbConnections.Count);
        }

        [Fact]
        public void Can_create_new_connection_from_exsting_DbConnection()
        {
            var dbConnection = new FakeDbConnection("Database=FrodoLives");

            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { Connection = dbConnection })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                Assert.Same(dbConnection, connection.DbConnection);

                Assert.Equal(0, connection.DbConnections.Count);
            }
        }

        [Fact]
        public void Existing_connection_is_opened_and_closed_when_necessary()
        {
            var dbConnection = new FakeDbConnection("Database=FrodoLives");

            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { Connection = dbConnection })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                connection.Open();

                Assert.Equal(0, connection.DbConnections.Count);

                Assert.Equal(1, dbConnection.OpenCount);

                connection.Open();
                connection.Open();

                Assert.Equal(1, dbConnection.OpenCount);

                connection.Close();
                connection.Close();

                Assert.Equal(1, dbConnection.OpenCount);
                Assert.Equal(0, dbConnection.CloseCount);

                connection.Close();

                Assert.Equal(1, dbConnection.OpenCount);
                Assert.Equal(1, dbConnection.CloseCount);

                connection.Open();

                Assert.Equal(2, dbConnection.OpenCount);

                connection.Close();

                Assert.Equal(2, dbConnection.OpenCount);
                Assert.Equal(2, dbConnection.CloseCount);
            }
        }

        [Fact]
        public void Existing_connection_can_start_in_opened_state()
        {
            var dbConnection = new FakeDbConnection(
                "Database=FrodoLives",
                state: ConnectionState.Open);

            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { Connection = dbConnection })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                connection.Open();

                Assert.Equal(0, connection.DbConnections.Count);

                Assert.Equal(0, dbConnection.OpenCount);

                connection.Open();
                connection.Open();

                Assert.Equal(0, dbConnection.OpenCount);

                connection.Close();
                connection.Close();

                Assert.Equal(0, dbConnection.OpenCount);
                Assert.Equal(0, dbConnection.CloseCount);

                connection.Close();

                Assert.Equal(0, dbConnection.OpenCount);
                Assert.Equal(0, dbConnection.CloseCount);

                connection.Open();

                Assert.Equal(0, dbConnection.OpenCount);

                connection.Close();

                Assert.Equal(0, dbConnection.OpenCount);
                Assert.Equal(0, dbConnection.CloseCount);
            }
        }

        [Fact]
        public void Existing_connection_can_be_opened_and_closed_externally()
        {
            var dbConnection = new FakeDbConnection(
                "Database=FrodoLives",
                state: ConnectionState.Closed);

            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { Connection = dbConnection })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                connection.Open();

                Assert.Equal(0, connection.DbConnections.Count);

                Assert.Equal(1, dbConnection.OpenCount);

                connection.Close();

                Assert.Equal(1, dbConnection.OpenCount);
                Assert.Equal(1, dbConnection.CloseCount);

                dbConnection.SetState(ConnectionState.Open);

                connection.Open();

                Assert.Equal(1, dbConnection.OpenCount);
                Assert.Equal(1, dbConnection.CloseCount);

                connection.Close();

                Assert.Equal(1, dbConnection.OpenCount);
                Assert.Equal(1, dbConnection.CloseCount);

                dbConnection.SetState(ConnectionState.Closed);

                connection.Open();

                Assert.Equal(2, dbConnection.OpenCount);
                Assert.Equal(1, dbConnection.CloseCount);

                connection.Close();

                Assert.Equal(2, dbConnection.OpenCount);
                Assert.Equal(2, dbConnection.CloseCount);
            }
        }

        [Fact]
        public void Existing_connection_is_not_disposed_even_after_being_opened_and_closed()
        {
            var dbConnection = new FakeDbConnection("Database=FrodoLives");
            var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { Connection = dbConnection }));

            Assert.Equal(0, connection.DbConnections.Count);
            Assert.Same(dbConnection, connection.DbConnection);

            connection.Open();
            connection.Close();
            connection.Dispose();

            Assert.Equal(1, dbConnection.OpenCount);
            Assert.Equal(1, dbConnection.CloseCount);
            Assert.Equal(0, dbConnection.DisposeCount);

            Assert.Equal(0, connection.DbConnections.Count);
            Assert.Same(dbConnection, connection.DbConnection);

            connection.Open();
            connection.Close();
            connection.Dispose();

            Assert.Equal(2, dbConnection.OpenCount);
            Assert.Equal(2, dbConnection.CloseCount);
            Assert.Equal(0, dbConnection.DisposeCount);
        }

        [Fact]
        public void Connection_is_opened_and_closed_by_using_transaction()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                var transaction = connection.BeginTransaction();

                Assert.Equal(1, connection.DbConnections.Count);
                var dbConnection = connection.DbConnections[0];

                Assert.Equal(1, dbConnection.DbTransactions.Count);
                var dbTransaction = dbConnection.DbTransactions[0];

                Assert.Equal(1, dbConnection.OpenCount);
                Assert.Equal(0, dbConnection.CloseCount);
                Assert.Equal(IsolationLevel.Unspecified, dbTransaction.IsolationLevel);

                transaction.Dispose();

                Assert.Equal(1, dbConnection.OpenCount);
                Assert.Equal(1, dbConnection.CloseCount);
            }
        }

        [Fact]
        public async Task Connection_is_opened_and_closed_by_using_transaction_async()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                var transaction = await connection.BeginTransactionAsync();

                Assert.Equal(1, connection.DbConnections.Count);
                var dbConnection = connection.DbConnections[0];

                Assert.Equal(1, dbConnection.DbTransactions.Count);
                var dbTransaction = dbConnection.DbTransactions[0];

                Assert.Equal(1, dbConnection.OpenCount);
                Assert.Equal(0, dbConnection.CloseCount);
                Assert.Equal(IsolationLevel.Unspecified, dbTransaction.IsolationLevel);

                transaction.Dispose();

                Assert.Equal(1, dbConnection.OpenCount);
                Assert.Equal(1, dbConnection.CloseCount);
            }
        }

        [Fact]
        public void Transaction_can_begin_with_isolation_level()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                using (var transaction = connection.BeginTransaction(IsolationLevel.Chaos))
                {
                    Assert.Equal(1, connection.DbConnections.Count);
                    var dbConnection = connection.DbConnections[0];

                    Assert.Equal(1, dbConnection.DbTransactions.Count);
                    var dbTransaction = dbConnection.DbTransactions[0];

                    Assert.Equal(IsolationLevel.Chaos, dbTransaction.IsolationLevel);
                }
            }
        }

        [Fact]
        public async Task Transaction_can_begin_with_isolation_level_async()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                using (var transaction = await connection.BeginTransactionAsync(IsolationLevel.Chaos))
                {
                    Assert.Equal(1, connection.DbConnections.Count);
                    var dbConnection = connection.DbConnections[0];

                    Assert.Equal(1, dbConnection.DbTransactions.Count);
                    var dbTransaction = dbConnection.DbTransactions[0];

                    Assert.Equal(IsolationLevel.Chaos, dbTransaction.IsolationLevel);
                }
            }
        }

        [Fact]
        public void Current_transaction_is_disposed_when_connection_is_disposed()
        {
            var connection = new FakeRelationalConnection(
                CreateOptions(
                    new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" }));

            Assert.Equal(0, connection.DbConnections.Count);

            var transaction = connection.BeginTransaction();

            Assert.Equal(1, connection.DbConnections.Count);
            var dbConnection = connection.DbConnections[0];

            Assert.Equal(1, dbConnection.DbTransactions.Count);
            var dbTransaction = dbConnection.DbTransactions[0];

            connection.Dispose();

            Assert.Equal(1, dbTransaction.DisposeCount);
            Assert.Null(connection.CurrentTransaction);
        }

        [Fact]
        public void Can_use_existing_transaction()
        {
            var dbConnection = new FakeDbConnection("Database=FrodoLives");

            var dbTransaction = dbConnection.BeginTransaction(IsolationLevel.Unspecified);

            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { Connection = dbConnection })))
            {
                using (connection.UseTransaction(dbTransaction))
                {
                    Assert.Equal(dbTransaction, connection.CurrentTransaction.GetDbTransaction());
                }
            }
        }

        [Fact]
        public void Commit_calls_commit_on_DbTransaction()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                using (var transaction = connection.BeginTransaction())
                {
                    Assert.Equal(1, connection.DbConnections.Count);
                    var dbConnection = connection.DbConnections[0];

                    Assert.Equal(1, dbConnection.DbTransactions.Count);
                    var dbTransaction = dbConnection.DbTransactions[0];

                    connection.CommitTransaction();

                    Assert.Equal(1, dbTransaction.CommitCount);
                }
            }
        }

        [Fact]
        public void Rollback_calls_rollback_on_DbTransaction()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                using (var transaction = connection.BeginTransaction())
                {
                    Assert.Equal(1, connection.DbConnections.Count);
                    var dbConnection = connection.DbConnections[0];

                    Assert.Equal(1, dbConnection.DbTransactions.Count);
                    var dbTransaction = dbConnection.DbTransactions[0];

                    connection.RollbackTransaction();

                    Assert.Equal(1, dbTransaction.RollbackCount);
                }
            }
        }

        [Fact]
        public void Can_create_new_connection_with_CommandTimeout()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension
                {
                    ConnectionString = "Database=FrodoLives",
                    CommandTimeout = 99
                })))
            {
                Assert.Equal(99, connection.CommandTimeout);
            }
        }

        [Fact]
        public void Can_set_CommandTimeout()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                connection.CommandTimeout = 88;

                Assert.Equal(88, connection.CommandTimeout);
            }
        }

        [Fact]
        public void Throws_if_CommandTimeout_out_of_range()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                Assert.Throws<ArgumentException>(
                    () => connection.CommandTimeout = -1);
            }
        }

        [Fact]
        public void Throws_if_no_relational_store_configured()
        {
            Assert.Equal(
                RelationalStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(
                    () => new FakeRelationalConnection(CreateOptions())).Message);
        }

        [Fact]
        public void Throws_if_multiple_relational_stores_configured()
        {
            Assert.Equal(
                RelationalStrings.MultipleProvidersConfigured,
                Assert.Throws<InvalidOperationException>(
                    () => new FakeRelationalConnection(
                        CreateOptions(
                            new FakeRelationalOptionsExtension(),
                            new FakeRelationalOptionsExtension()))).Message);
        }

        [Fact]
        public void Throws_if_no_connection_or_connection_string_is_specified()
        {
            Assert.Equal(
                RelationalStrings.NoConnectionOrConnectionString,
                Assert.Throws<InvalidOperationException>(
                    () => new FakeRelationalConnection(
                        CreateOptions(
                            new FakeRelationalOptionsExtension()))).Message);
        }

        [Fact]
        public void Throws_if_both_connection_and_connection_string_are_specified()
        {
            Assert.Equal(
                RelationalStrings.ConnectionAndConnectionString,
                Assert.Throws<InvalidOperationException>(() => new FakeRelationalConnection(
                    CreateOptions(new FakeRelationalOptionsExtension
                    {
                        Connection = new FakeDbConnection("Database=FrodoLives"),
                        ConnectionString = "Database=FrodoLives"
                    }))).Message);
        }

        [Fact]
        public void Throws_when_commit_is_called_without_active_transaction()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                Assert.Equal(
                    RelationalStrings.NoActiveTransaction,
                    Assert.Throws<InvalidOperationException>(
                        () => connection.CommitTransaction()).Message);
            }
        }

        [Fact]
        public void Throws_when_rollback_is_called_without_active_transaction()
        {
            using (var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension { ConnectionString = "Database=FrodoLives" })))
            {
                Assert.Equal(0, connection.DbConnections.Count);

                Assert.Equal(
                    RelationalStrings.NoActiveTransaction,
                    Assert.Throws<InvalidOperationException>(
                        () => connection.RollbackTransaction()).Message);
            }
        }

        private static IDbContextOptions CreateOptions(params RelationalOptionsExtension[] optionsExtensions)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            foreach (var optionsExtension in optionsExtensions)
            {
                ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(optionsExtension);
            }

            return optionsBuilder.Options;
        }
    }
}
