// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class RelationalDatabaseFacadeExtensionsTest
    {
        [ConditionalFact]
        public void GetDbConnection_returns_the_current_connection()
        {
            var dbConnection = new FakeDbConnection("A=B");
            var context = RelationalTestHelpers.Instance.CreateContext();

            ((FakeRelationalConnection)context.GetService<IRelationalConnection>()).UseConnection(dbConnection);

            Assert.Same(dbConnection, context.Database.GetDbConnection());
        }

        [ConditionalFact]
        public void Relational_specific_methods_throws_when_non_relational_provider_is_in_use()
        {
            var optionsBuilder = new DbContextOptionsBuilder()
                .UseInternalServiceProvider(
                    new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider())
                .UseInMemoryDatabase(Guid.NewGuid().ToString());
            var context = new DbContext(optionsBuilder.Options);

            Assert.Equal(
                RelationalStrings.RelationalNotInUse,
                Assert.Throws<InvalidOperationException>(() => context.Database.GetDbConnection()).Message);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_open_the_underlying_connection(bool async)
        {
            var dbConnection = new FakeDbConnection("A=B");
            var context = RelationalTestHelpers.Instance.CreateContext();

            ((FakeRelationalConnection)context.GetService<IRelationalConnection>()).UseConnection(dbConnection);

            if (async)
            {
                await context.Database.OpenConnectionAsync();
                Assert.Equal(1, dbConnection.OpenAsyncCount);
            }
            else
            {
                context.Database.OpenConnection();
                Assert.Equal(1, dbConnection.OpenCount);
            }
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_close_the_underlying_connection(bool async)
        {
            var dbConnection = new FakeDbConnection("A=B");
            var context = RelationalTestHelpers.Instance.CreateContext();

            ((FakeRelationalConnection)context.GetService<IRelationalConnection>()).UseConnection(dbConnection);

            if (async)
            {
                await context.Database.OpenConnectionAsync();
                await context.Database.CloseConnectionAsync();
            }
            else
            {
                context.Database.OpenConnection();
                context.Database.CloseConnection();
            }

            Assert.Equal(1, dbConnection.CloseCount);
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_begin_transaction_with_isolation_level(bool async)
        {
            var dbConnection = new FakeDbConnection("A=B");
            var context = RelationalTestHelpers.Instance.CreateContext();
            ((FakeRelationalConnection)context.GetService<IRelationalConnection>()).UseConnection(dbConnection);

            var transaction = async
                ? await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Chaos)
                : context.Database.BeginTransaction(System.Data.IsolationLevel.Chaos);

            Assert.Same(dbConnection.DbTransactions.Single(), transaction.GetDbTransaction());
            Assert.Equal(System.Data.IsolationLevel.Chaos, transaction.GetDbTransaction().IsolationLevel);
        }

        [ConditionalFact]
        public void Can_use_transaction()
        {
            var dbConnection = new FakeDbConnection("A=B");
            var context = RelationalTestHelpers.Instance.CreateContext();
            ((FakeRelationalConnection)context.GetService<IRelationalConnection>()).UseConnection(dbConnection);
            var transaction = new FakeDbTransaction(dbConnection, System.Data.IsolationLevel.Chaos);

            Assert.Same(transaction, context.Database.UseTransaction(transaction).GetDbTransaction());
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Begin_transaction_ignores_isolation_level_on_non_relational_provider(bool async)
        {
            var context = InMemoryTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddScoped<IDbContextTransactionManager, FakeDbContextTransactionManager>());

            var transactionManager = (FakeDbContextTransactionManager)context.GetService<IDbContextTransactionManager>();

            if (async)
            {
                await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Chaos);
                Assert.Equal(1, transactionManager.BeginAsyncCount);
            }
            else
            {
                context.Database.BeginTransaction(System.Data.IsolationLevel.Chaos);
                Assert.Equal(1, transactionManager.BeginCount);
            }
        }

        private class FakeDbContextTransactionManager : IDbContextTransactionManager
        {
            public int BeginCount { get; set; }
            public int BeginAsyncCount { get; set; }

            public void ResetState()
            {
            }

            public Task ResetStateAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public IDbContextTransaction BeginTransaction()
            {
                BeginCount++;
                return new InMemoryTransaction();
            }

            public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
            {
                BeginAsyncCount++;
                return Task.FromResult<IDbContextTransaction>(new InMemoryTransaction());
            }

            public void CommitTransaction()
            {
            }

            public void RollbackTransaction()
            {
            }

            public IDbContextTransaction CurrentTransaction { get; }

            public Transaction EnlistedTransaction { get; }

            public void EnlistTransaction(Transaction transaction)
            {
            }
        }

        [ConditionalFact]
        public void use_transaction_throws_on_non_relational_provider()
        {
            var transaction = new FakeDbTransaction(new FakeDbConnection("A=B"));
            var context = InMemoryTestHelpers.Instance.CreateContext();

            Assert.Equal(
                RelationalStrings.RelationalNotInUse,
                Assert.Throws<InvalidOperationException>(
                    () => context.Database.UseTransaction(transaction)).Message);
        }

        [ConditionalFact]
        public void GetMigrations_works()
        {
            var migrations = new[]
            {
                "00000000000001_One",
                "00000000000002_Two",
                "00000000000003_Three"
            };

            var migrationsAssembly = new FakeIMigrationsAssembly
            {
                Migrations = migrations.ToDictionary(x => x, x => default(TypeInfo))
            };

            var db = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton<IMigrationsAssembly>(migrationsAssembly));

            Assert.Equal(migrations, db.Database.GetMigrations());
        }

        private class FakeIMigrationsAssembly : IMigrationsAssembly
        {
            public IReadOnlyDictionary<string, TypeInfo> Migrations { get; set; }
            public ModelSnapshot ModelSnapshot { get; }
            public Assembly Assembly { get; }
            public string FindMigrationId(string nameOrId) => throw new NotImplementedException();
            public Migration CreateMigration(TypeInfo migrationClass, string activeProvider) => throw new NotImplementedException();
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetAppliedMigrations_works(bool async)
        {
            var migrations = new[]
            {
                "00000000000001_One",
                "00000000000002_Two"
            };

            var repository = new FakeHistoryRepository
            {
                AppliedMigrations = migrations.Select(id => new HistoryRow(id, "1.1.0")).ToList()
            };

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection().AddSingleton<IHistoryRepository>(repository));

            Assert.Equal(
                migrations,
                async
                    ? await context.Database.GetAppliedMigrationsAsync()
                    : context.Database.GetAppliedMigrations());
        }

        private class FakeHistoryRepository : IHistoryRepository
        {
            public List<HistoryRow> AppliedMigrations { get; set; }

            public IReadOnlyList<HistoryRow> GetAppliedMigrations()
                => AppliedMigrations;

            public Task<IReadOnlyList<HistoryRow>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IReadOnlyList<HistoryRow>>(AppliedMigrations);

            public bool Exists() => throw new NotImplementedException();
            public Task<bool> ExistsAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public string GetCreateScript() => throw new NotImplementedException();
            public string GetCreateIfNotExistsScript() => throw new NotImplementedException();
            public string GetInsertScript(HistoryRow row) => throw new NotImplementedException();
            public string GetDeleteScript(string migrationId) => throw new NotImplementedException();
            public string GetBeginIfNotExistsScript(string migrationId) => throw new NotImplementedException();
            public string GetBeginIfExistsScript(string migrationId) => throw new NotImplementedException();
            public string GetEndIfScript() => throw new NotImplementedException();
        }

        [ConditionalTheory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task GetPendingMigrations_works(bool async)
        {
            var migrations = new[]
            {
                "00000000000001_One",
                "00000000000002_Two",
                "00000000000003_Three"
            };

            var appliedMigrations = new[]
            {
                "00000000000001_One",
                "00000000000002_Two"
            };

            var migrationsAssembly = new FakeIMigrationsAssembly
            {
                Migrations = migrations.ToDictionary(x => x, x => default(TypeInfo))
            };

            var repository = new FakeHistoryRepository
            {
                AppliedMigrations = appliedMigrations.Select(id => new HistoryRow(id, "1.1.0")).ToList()
            };

            var context = RelationalTestHelpers.Instance.CreateContext(
                new ServiceCollection()
                    .AddSingleton<IHistoryRepository>(repository)
                    .AddSingleton<IMigrationsAssembly>(migrationsAssembly));

            Assert.Equal(
                new[] { "00000000000003_Three" },
                async
                    ? await context.Database.GetPendingMigrationsAsync()
                    : context.Database.GetPendingMigrations());
        }
    }
}
