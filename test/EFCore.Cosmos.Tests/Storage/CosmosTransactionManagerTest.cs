// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Storage;

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
