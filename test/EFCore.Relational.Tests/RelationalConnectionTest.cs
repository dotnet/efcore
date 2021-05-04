// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class RelationalConnectionTest
    {
        [ConditionalFact]
        public void Throws_with_new_when_no_EF_services_use_Database()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseInternalServiceProvider(new ServiceCollection().BuildServiceProvider())
                .Options;

            Assert.Equal(
                CoreStrings.NoEfServices,
                Assert.Throws<InvalidOperationException>(() => new ConstructorTestContext1A(options)).Message);
        }

        [ConditionalFact]
        public void Throws_with_add_when_no_EF_services_use_Database()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContext1A>(
                    (p, b) => b.UseInternalServiceProvider(p))
                .BuildServiceProvider();

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            Assert.Equal(
                CoreStrings.NoEfServices,
                Assert.Throws<InvalidOperationException>(
                    () => serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>()).Message);
        }

        [ConditionalFact]
        public void Throws_with_new_when_no_provider_use_Database()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using var context = new ConstructorTestContext1A(options);
            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Database.GetDbConnection()).Message);
        }

        [ConditionalFact]
        public void Throws_with_add_when_no_provider_use_Database()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

            var appServiceProvider = serviceCollection
                .AddDbContext<ConstructorTestContext1A>(
                    (p, b) => b.UseInternalServiceProvider(p))
                .BuildServiceProvider();

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Database.GetDbConnection()).Message);
        }

        [ConditionalFact]
        public void Throws_with_new_when_no_EF_services_because_parameterless_constructor_use_Database()
        {
            using var context = new ConstructorTestContextNoConfiguration();
            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Database.GetDbConnection()).Message);
        }

        [ConditionalFact]
        public void Throws_with_add_when_no_EF_services_because_parameterless_constructor_use_Database()
        {
            var appServiceProvider = new ServiceCollection()
                .AddDbContext<ConstructorTestContextNoConfiguration>()
                .BuildServiceProvider();

            using var serviceScope = appServiceProvider
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope();
            var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextNoConfiguration>();

            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Database.GetDbConnection()).Message);
        }

        private class ConstructorTestContext1A : DbContext
        {
            public ConstructorTestContext1A(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class ConstructorTestContextNoConfiguration : DbContext
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInternalServiceProvider(
                    new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider());
        }

        [ConditionalFact]
        public void Can_create_new_connection_lazily_using_given_connection_string()
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));
            Assert.Equal(0, connection.DbConnections.Count);

            var dbConnection = connection.DbConnection;

            Assert.Equal(1, connection.DbConnections.Count);
            Assert.Equal("Database=FrodoLives", dbConnection.ConnectionString);
        }

        [ConditionalFact]
        public void Can_change_or_reset_connection_string()
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));

            connection.ConnectionString = null;
            Assert.Null(connection.ConnectionString);

            connection.ConnectionString = "Database=SamLives";
            Assert.Equal("Database=SamLives", connection.ConnectionString);

            Assert.Equal(0, connection.DbConnections.Count);

            var dbConnection = connection.DbConnection;

            Assert.Equal(1, connection.DbConnections.Count);
            Assert.Equal("Database=SamLives", connection.ConnectionString);
            Assert.Equal("Database=SamLives", dbConnection.ConnectionString);

            connection.ConnectionString = null;

            Assert.Equal(1, connection.DbConnections.Count);
            Assert.Null(connection.ConnectionString);
            Assert.Null(dbConnection.ConnectionString);

            connection.ConnectionString = "Database=MerryLives";

            dbConnection = connection.DbConnection;

            Assert.Equal(1, connection.DbConnections.Count);
            Assert.Equal("Database=MerryLives", connection.ConnectionString);
            Assert.Equal("Database=MerryLives", dbConnection.ConnectionString);
        }

        [ConditionalFact]
        public void Lazy_connection_is_opened_and_closed_when_necessary()
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));
            Assert.Equal(0, connection.DbConnections.Count);

            Assert.True(connection.Open());

            Assert.Equal(1, connection.DbConnections.Count);

            var dbConnection = connection.DbConnections[0];
            Assert.Equal(1, dbConnection.OpenCount);

            Assert.False(connection.Open());
            Assert.False(connection.Open());

            Assert.Equal(1, dbConnection.OpenCount);

            Assert.False(connection.Close());
            Assert.False(connection.Close());

            Assert.Equal(1, dbConnection.OpenCount);
            Assert.Equal(0, dbConnection.CloseCount);

            Assert.True(connection.Close());

            Assert.Equal(1, dbConnection.OpenCount);
            Assert.Equal(1, dbConnection.CloseCount);

            Assert.True(connection.Open());

            Assert.Equal(2, dbConnection.OpenCount);

            Assert.True(connection.Close());

            Assert.Equal(2, dbConnection.OpenCount);
            Assert.Equal(2, dbConnection.CloseCount);
        }

        [ConditionalFact]
        public async Task Lazy_connection_is_async_opened_and_closed_when_necessary()
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));
            Assert.Equal(0, connection.DbConnections.Count);

            var cancellationToken = new CancellationTokenSource().Token;
            Assert.True(await connection.OpenAsync(cancellationToken));

            Assert.Equal(1, connection.DbConnections.Count);

            var dbConnection = connection.DbConnections[0];
            Assert.Equal(1, dbConnection.OpenAsyncCount);

            Assert.False(await connection.OpenAsync(cancellationToken));
            Assert.False(await connection.OpenAsync(cancellationToken));

            Assert.Equal(1, dbConnection.OpenAsyncCount);

            Assert.False(connection.Close());
            Assert.False(connection.Close());

            Assert.Equal(1, dbConnection.OpenAsyncCount);
            Assert.Equal(0, dbConnection.CloseCount);

            Assert.True(connection.Close());

            Assert.Equal(1, dbConnection.OpenAsyncCount);
            Assert.Equal(1, dbConnection.CloseCount);

            Assert.True(await connection.OpenAsync(cancellationToken));

            Assert.Equal(2, dbConnection.OpenAsyncCount);

            Assert.True(connection.Close());

            Assert.Equal(2, dbConnection.OpenAsyncCount);
            Assert.Equal(2, dbConnection.CloseCount);
        }

        [ConditionalFact]
        public void Lazy_connection_is_recreated_if_used_again_after_being_disposed()
        {
            var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));

            Assert.Equal(0, connection.DbConnections.Count);
            var dbConnection = (FakeDbConnection)connection.DbConnection;
            Assert.Equal(1, connection.DbConnections.Count);

            connection.Open();
            Assert.Equal(1, dbConnection.OpenCount);

            connection.Close();
            Assert.Equal(1, dbConnection.CloseCount);

            connection.Dispose();
            Assert.Equal(1, dbConnection.DisposeCount);

            Assert.Equal(1, connection.DbConnections.Count);
            dbConnection = (FakeDbConnection)connection.DbConnection;
            Assert.Equal(2, connection.DbConnections.Count);

            connection.Open();
            Assert.Equal(1, dbConnection.OpenCount);

            connection.Close();
            Assert.Equal(1, dbConnection.CloseCount);

            connection.Dispose();
            Assert.Equal(1, dbConnection.DisposeCount);
        }

        [ConditionalFact]
        public void Lazy_connection_is_not_created_just_so_it_can_be_disposed()
        {
            var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));

            connection.Dispose();

            Assert.Equal(0, connection.DbConnections.Count);
        }

        [ConditionalFact]
        public void Can_create_new_connection_from_existing_DbConnection()
        {
            var dbConnection = new FakeDbConnection("Database=FrodoLives");

            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnection(dbConnection)));
            Assert.Equal(0, connection.DbConnections.Count);

            Assert.Same(dbConnection, connection.DbConnection);

            Assert.Equal(0, connection.DbConnections.Count);
        }

        [ConditionalFact]
        public void Existing_connection_is_opened_and_closed_when_necessary()
        {
            var dbConnection = new FakeDbConnection("Database=FrodoLives");

            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnection(dbConnection)));
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

        [ConditionalFact]
        public void Existing_connection_can_start_in_opened_state()
        {
            var dbConnection = new FakeDbConnection(
                "Database=FrodoLives",
                state: ConnectionState.Open);

            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnection(dbConnection)));
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

        [ConditionalFact]
        public void Existing_connection_can_be_opened_and_closed_externally()
        {
            var dbConnection = new FakeDbConnection(
                "Database=FrodoLives");

            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnection(dbConnection)));
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

            dbConnection.SetState(ConnectionState.Open);

            connection.Open();

            Assert.Equal(2, dbConnection.OpenCount);
            Assert.Equal(2, dbConnection.CloseCount);

            dbConnection.SetState(ConnectionState.Closed);

            connection.Close();

            Assert.Equal(2, dbConnection.OpenCount);
            Assert.Equal(2, dbConnection.CloseCount);

            connection.Open();
            connection.Open();

            Assert.Equal(3, dbConnection.OpenCount);

            dbConnection.SetState(ConnectionState.Closed);

            connection.Open();

            Assert.Equal(4, dbConnection.OpenCount);
            Assert.Equal(2, dbConnection.CloseCount);

            dbConnection.SetState(ConnectionState.Closed);

            connection.Close();

            Assert.Equal(4, dbConnection.OpenCount);
            Assert.Equal(2, dbConnection.CloseCount);
        }

        [ConditionalFact]
        public void Existing_connection_can_be_changed_and_reset()
        {
            var dbConnection = new FakeDbConnection("Database=FrodoLives");

            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnection(dbConnection)));

            Assert.Equal(0, connection.DbConnections.Count);

            connection.DbConnection = null;
            Assert.Null(connection.ConnectionString);

            dbConnection = new FakeDbConnection("Database=SamLives");
            connection.DbConnection = dbConnection;

            Assert.Equal("Database=SamLives", connection.ConnectionString);

            Assert.Equal(0, connection.DbConnections.Count);
            Assert.Same(dbConnection, connection.DbConnection);
            Assert.Equal(0, connection.DbConnections.Count);
            Assert.Equal("Database=SamLives", connection.ConnectionString);

            connection.DbConnection = null;

            Assert.Equal(0, connection.DbConnections.Count);
            Assert.Null(connection.ConnectionString);

            connection.ConnectionString = "Database=MerryLives";

            dbConnection = new FakeDbConnection("Database=MerryLives");
            connection.DbConnection = dbConnection;

            Assert.Equal(0, connection.DbConnections.Count);
            Assert.Same(dbConnection, connection.DbConnection);
            Assert.Equal(0, connection.DbConnections.Count);
            Assert.Equal("Database=MerryLives", connection.ConnectionString);
        }

        [ConditionalFact]
        public async Task Existing_connection_can_be_opened_and_closed_externally_async()
        {
            var dbConnection = new FakeDbConnection(
                "Database=FrodoLives");

            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnection(dbConnection)));
            Assert.Equal(0, connection.DbConnections.Count);

            await connection.OpenAsync(default);

            Assert.Equal(0, connection.DbConnections.Count);

            Assert.Equal(1, dbConnection.OpenCount);

            await connection.CloseAsync();

            Assert.Equal(1, dbConnection.OpenCount);
            Assert.Equal(1, dbConnection.CloseCount);

            dbConnection.SetState(ConnectionState.Open);

            await connection.OpenAsync(default);

            Assert.Equal(1, dbConnection.OpenCount);
            Assert.Equal(1, dbConnection.CloseCount);

            await connection.CloseAsync();

            Assert.Equal(1, dbConnection.OpenCount);
            Assert.Equal(1, dbConnection.CloseCount);

            dbConnection.SetState(ConnectionState.Closed);

            await connection.OpenAsync(default);

            Assert.Equal(2, dbConnection.OpenCount);
            Assert.Equal(1, dbConnection.CloseCount);

            await connection.CloseAsync();

            Assert.Equal(2, dbConnection.OpenCount);
            Assert.Equal(2, dbConnection.CloseCount);

            dbConnection.SetState(ConnectionState.Open);

            await connection.OpenAsync(default);

            Assert.Equal(2, dbConnection.OpenCount);
            Assert.Equal(2, dbConnection.CloseCount);

            dbConnection.SetState(ConnectionState.Closed);

            await connection.CloseAsync();

            Assert.Equal(2, dbConnection.OpenCount);
            Assert.Equal(2, dbConnection.CloseCount);

            await connection.OpenAsync(default);
            await connection.OpenAsync(default);

            Assert.Equal(3, dbConnection.OpenCount);

            dbConnection.SetState(ConnectionState.Closed);

            await connection.OpenAsync(default);

            Assert.Equal(4, dbConnection.OpenCount);
            Assert.Equal(2, dbConnection.CloseCount);

            dbConnection.SetState(ConnectionState.Closed);

            await connection.CloseAsync();

            Assert.Equal(4, dbConnection.OpenCount);
            Assert.Equal(2, dbConnection.CloseCount);
        }

        [ConditionalFact]
        public void Existing_connection_is_not_disposed_even_after_being_opened_and_closed()
        {
            var dbConnection = new FakeDbConnection("Database=FrodoLives");
            var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnection(dbConnection)));

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

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Connection_is_opened_and_closed_by_using_transaction(bool async)
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));
            Assert.Equal(0, connection.DbConnections.Count);

            Assert.Null(connection.CurrentTransaction);

            var transaction = async
                ? await connection.BeginTransactionAsync()
                : connection.BeginTransaction();

            Assert.Same(transaction, connection.CurrentTransaction);

            Assert.Equal(1, connection.DbConnections.Count);
            var dbConnection = connection.DbConnections[0];

            Assert.Equal(1, dbConnection.DbTransactions.Count);
            var dbTransaction = dbConnection.DbTransactions[0];

            Assert.Equal(1, dbConnection.OpenCount);
            Assert.Equal(0, dbConnection.CloseCount);
            Assert.Equal(IsolationLevel.Unspecified, dbTransaction.IsolationLevel);

            transaction.Dispose();

            Assert.Null(connection.CurrentTransaction);

            Assert.Equal(1, dbConnection.OpenCount);
            Assert.Equal(1, dbConnection.CloseCount);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Transaction_can_begin_with_isolation_level(bool async)
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));
            Assert.Equal(0, connection.DbConnections.Count);

            Assert.Null(connection.CurrentTransaction);

            var transaction = async
                ? await connection.BeginTransactionAsync(IsolationLevel.Chaos)
                : connection.BeginTransaction(IsolationLevel.Chaos);

            using (transaction)
            {
                Assert.Same(transaction, connection.CurrentTransaction);

                Assert.Equal(1, connection.DbConnections.Count);
                var dbConnection = connection.DbConnections[0];

                Assert.Equal(1, dbConnection.DbTransactions.Count);
                var dbTransaction = dbConnection.DbTransactions[0];

                Assert.Equal(IsolationLevel.Chaos, dbTransaction.IsolationLevel);
            }

            Assert.Null(connection.CurrentTransaction);
        }

        [ConditionalFact]
        public void Current_transaction_is_disposed_when_connection_is_disposed()
        {
            var connection = new FakeRelationalConnection(
                CreateOptions(
                    new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));

            Assert.Equal(0, connection.DbConnections.Count);

            Assert.Null(connection.CurrentTransaction);

            var transaction = connection.BeginTransaction();

            Assert.Same(transaction, connection.CurrentTransaction);

            Assert.Equal(1, connection.DbConnections.Count);
            var dbConnection = connection.DbConnections[0];

            Assert.Equal(1, dbConnection.DbTransactions.Count);
            var dbTransaction = dbConnection.DbTransactions[0];

            connection.Dispose();

            Assert.Equal(1, dbTransaction.DisposeCount);
            Assert.Null(connection.CurrentTransaction);
        }

        [ConditionalFact]
        public void Can_use_existing_transaction()
        {
            var dbConnection = new FakeDbConnection("Database=FrodoLives");

            var dbTransaction = dbConnection.BeginTransaction(IsolationLevel.Unspecified);

            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnection(dbConnection)));
            Assert.Null(connection.CurrentTransaction);

            using (connection.UseTransaction(dbTransaction))
            {
                Assert.Equal(dbTransaction, connection.CurrentTransaction.GetDbTransaction());
            }

            Assert.Null(connection.CurrentTransaction);
        }

        [ConditionalFact]
        public void Can_use_existing_transaction_identifier()
        {
            var dbConnection = new FakeDbConnection("Database=FrodoLives");

            var dbTransaction = dbConnection.BeginTransaction(IsolationLevel.Unspecified);

            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnection(dbConnection)));
            Assert.Null(connection.CurrentTransaction);

            var transactionId = Guid.NewGuid();

            using (var transaction = connection.UseTransaction(dbTransaction, transactionId))
            {
                Assert.Equal(dbTransaction, connection.CurrentTransaction.GetDbTransaction());
                Assert.Equal(transactionId, transaction.TransactionId);
            }

            Assert.Null(connection.CurrentTransaction);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Commit_calls_Commit_on_DbTransaction(bool async)
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));
            Assert.Equal(0, connection.DbConnections.Count);

            Assert.Null(connection.CurrentTransaction);

            using (var transaction = connection.BeginTransaction())
            {
                Assert.Same(transaction, connection.CurrentTransaction);

                Assert.Equal(1, connection.DbConnections.Count);
                var dbConnection = connection.DbConnections[0];

                Assert.Equal(1, dbConnection.DbTransactions.Count);
                var dbTransaction = dbConnection.DbTransactions[0];

                if (async)
                {
                    await connection.CommitTransactionAsync();
                }
                else
                {
                    connection.CommitTransaction();
                }

                Assert.Equal(1, dbTransaction.CommitCount);
            }

            Assert.Null(connection.CurrentTransaction);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Rollback_calls_Rollback_on_DbTransaction(bool async)
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));
            Assert.Equal(0, connection.DbConnections.Count);

            Assert.Null(connection.CurrentTransaction);

            using (var transaction = connection.BeginTransaction())
            {
                Assert.Same(transaction, connection.CurrentTransaction);

                Assert.Equal(1, connection.DbConnections.Count);
                var dbConnection = connection.DbConnections[0];

                Assert.Equal(1, dbConnection.DbTransactions.Count);
                var dbTransaction = dbConnection.DbTransactions[0];

                if (async)
                {
                    await connection.RollbackTransactionAsync();
                }
                else
                {
                    connection.RollbackTransaction();
                }

                Assert.Equal(1, dbTransaction.RollbackCount);
            }

            Assert.Null(connection.CurrentTransaction);
        }

        [ConditionalFact]
        public void Can_create_new_connection_with_CommandTimeout()
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(
                    new FakeRelationalOptionsExtension()
                        .WithConnectionString("Database=FrodoLives")
                        .WithCommandTimeout(99)));
            Assert.Equal(99, connection.CommandTimeout);
        }

        [ConditionalFact]
        public void Can_set_CommandTimeout()
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));
            connection.CommandTimeout = 88;

            Assert.Equal(88, connection.CommandTimeout);
        }

        [ConditionalFact]
        public void Throws_if_CommandTimeout_out_of_range()
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));
            Assert.Throws<ArgumentException>(
                () => connection.CommandTimeout = -1);
        }

        [ConditionalFact]
        public void Throws_if_no_relational_store_configured()
        {
            Assert.Equal(
                RelationalStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(
                    () => new FakeRelationalConnection(CreateOptions())).Message);
        }

        [ConditionalFact]
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

        [ConditionalFact]
        public void Throws_if_no_connection_or_connection_string_is_specified_only_when_accessed()
        {
            var connection = new FakeRelationalConnection(CreateOptions(new FakeRelationalOptionsExtension()));

            Assert.Equal(
                RelationalStrings.NoConnectionOrConnectionString,
                Assert.Throws<InvalidOperationException>(
                    () => connection.DbConnection).Message);

            Assert.Null(connection.ConnectionString);
        }

        [ConditionalFact]
        public void Puts_connection_string_on_connection_if_both_are_specified()
        {
            var connection = new FakeRelationalConnection(
                CreateOptions(
                    new FakeRelationalOptionsExtension()
                        .WithConnection(new FakeDbConnection("Database=FrodoLives"))
                        .WithConnectionString("Database=SamLives")));

            Assert.Equal("Database=SamLives", connection.DbConnection.ConnectionString);
        }

        [ConditionalFact]
        public void Throws_when_commit_is_called_without_active_transaction()
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));
            Assert.Equal(0, connection.DbConnections.Count);

            Assert.Equal(
                RelationalStrings.NoActiveTransaction,
                Assert.Throws<InvalidOperationException>(
                    () => connection.CommitTransaction()).Message);
        }

        [ConditionalFact]
        public void Throws_when_rollback_is_called_without_active_transaction()
        {
            using var connection = new FakeRelationalConnection(
                CreateOptions(new FakeRelationalOptionsExtension().WithConnectionString("Database=FrodoLives")));
            Assert.Equal(0, connection.DbConnections.Count);

            Assert.Equal(
                RelationalStrings.NoActiveTransaction,
                Assert.Throws<InvalidOperationException>(
                    () => connection.RollbackTransaction()).Message);
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
