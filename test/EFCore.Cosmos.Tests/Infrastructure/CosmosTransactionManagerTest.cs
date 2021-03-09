// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class CosmosTransactionManagerTest
    {
        [ConditionalFact]
        public virtual async Task CosmosTransactionManager_does_not_support_transactions()
        {
            var transactionManager = new CosmosTransactionManager();

            Assert.Equal(
                CosmosStrings.TransactionsNotSupported,
                Assert.Throws<NotSupportedException>(() => transactionManager.BeginTransaction()).Message);

            Assert.Equal(
                CosmosStrings.TransactionsNotSupported,
                (await Assert.ThrowsAsync<NotSupportedException>(async () => await transactionManager.BeginTransactionAsync())).Message);

            Assert.Equal(
                CosmosStrings.TransactionsNotSupported,
                Assert.Throws<NotSupportedException>(() => transactionManager.CommitTransaction()).Message);

            Assert.Equal(
                CosmosStrings.TransactionsNotSupported,
                (await Assert.ThrowsAsync<NotSupportedException>(async () => await transactionManager.CommitTransactionAsync())).Message);

            Assert.Equal(
                CosmosStrings.TransactionsNotSupported,
                Assert.Throws<NotSupportedException>(() => transactionManager.RollbackTransaction()).Message);

            Assert.Equal(
                CosmosStrings.TransactionsNotSupported,
                (await Assert.ThrowsAsync<NotSupportedException>(async () => await transactionManager.RollbackTransactionAsync())).Message);

            Assert.Null(transactionManager.CurrentTransaction);
            Assert.Null(transactionManager.EnlistedTransaction);

            Assert.Equal(
                CosmosStrings.TransactionsNotSupported,
                Assert.Throws<NotSupportedException>(() => transactionManager.EnlistTransaction(null)).Message);

            transactionManager.ResetState();
            await transactionManager.ResetStateAsync();

            Assert.Null(transactionManager.CurrentTransaction);
            Assert.Null(transactionManager.EnlistedTransaction);
        }
    }
}
