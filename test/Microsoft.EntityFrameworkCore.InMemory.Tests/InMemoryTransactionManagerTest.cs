// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Xunit;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryTransactionManagerTest
    {
        [Fact]
        public void Throws_on_BeginTransaction()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();

            var transactionManager = new InMemoryTransactionManager(optionsBuilder.Options);

            Assert.Equal(
                InMemoryStrings.TransactionsNotSupported,
                Assert.Throws<InvalidOperationException>(
                    () => transactionManager.BeginTransaction()).Message);
        }

        [Fact]
        public async Task Throws_on_BeginTransactionAsync()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();

            var transactionManager = new InMemoryTransactionManager(optionsBuilder.Options);

            Assert.Equal(
                InMemoryStrings.TransactionsNotSupported,
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () => await transactionManager.BeginTransactionAsync())).Message);
        }

        [Fact]
        public void Throws_on_CommitTransaction()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();

            var transactionManager = new InMemoryTransactionManager(optionsBuilder.Options);

            Assert.Equal(
                InMemoryStrings.TransactionsNotSupported,
                Assert.Throws<InvalidOperationException>(
                    () => transactionManager.CommitTransaction()).Message);
        }

        [Fact]
        public void Throws_on_RollbackTransaction()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase();

            var transactionManager = new InMemoryTransactionManager(optionsBuilder.Options);

            Assert.Equal(
                InMemoryStrings.TransactionsNotSupported,
                Assert.Throws<InvalidOperationException>(
                    () => transactionManager.RollbackTransaction()).Message);
        }

        [Fact]
        public void Does_not_throw_on_BeginTransaction_when_transactions_ignored()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase()
                .IgnoreTransactions();

            var transactionManager = new InMemoryTransactionManager(optionsBuilder.Options);

            using (var transaction = transactionManager.BeginTransaction())
            {
                transaction.Commit();
                transaction.Rollback();
            }
        }

        [Fact]
        public async Task Does_not_throw_on_BeginTransactionAsync_when_transactions_ignored()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase()
                .IgnoreTransactions();

            var transactionManager = new InMemoryTransactionManager(optionsBuilder.Options);

            using (var transaction = await transactionManager.BeginTransactionAsync())
            {
                transaction.Commit();
                transaction.Rollback();
            }
        }

        [Fact]
        public void Does_not_throw_on_CommitTransaction_when_transactions_ignored()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase()
                .IgnoreTransactions();

            var transactionManager = new InMemoryTransactionManager(optionsBuilder.Options);

            transactionManager.CommitTransaction();
        }

        [Fact]
        public void Does_not_throw_on_RollbackTransaction_when_transactions_ignored()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseInMemoryDatabase()
                .IgnoreTransactions();

            var transactionManager = new InMemoryTransactionManager(optionsBuilder.Options);

            transactionManager.RollbackTransaction();
        }
    }
}
