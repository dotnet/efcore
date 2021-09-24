// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace System.Transactions
{
    /// <summary>
    ///     Extension methods for the <see cref="DatabaseFacade" /> returned from <see cref="DbContext.Database" />
    ///     for use with <see cref="Transaction" />.
    /// </summary>
    public static class TransactionsDatabaseFacadeExtensions
    {
        /// <summary>
        ///     Specifies an existing <see cref="Transaction" /> to be used for database operations.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information.
        /// </remarks>
        /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
        /// <param name="transaction">The transaction to be used.</param>
        public static void EnlistTransaction(this DatabaseFacade databaseFacade, Transaction? transaction)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));
            if (((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies.TransactionManager is ITransactionEnlistmentManager
                transactionManager)
            {
                transactionManager.EnlistTransaction(transaction);
            }
            else
            {
                throw new NotSupportedException(CoreStrings.TransactionsNotSupported);
            }
        }

        /// <summary>
        ///     Returns the currently enlisted transaction.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information.
        /// </remarks>
        /// <param name="databaseFacade">The <see cref="DatabaseFacade" /> for the context.</param>
        /// <returns>The currently enlisted transaction.</returns>
        public static Transaction? GetEnlistedTransaction(this DatabaseFacade databaseFacade)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));
            if (((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies.TransactionManager is ITransactionEnlistmentManager
                transactionManager)
            {
                return transactionManager.EnlistedTransaction;
            }

            throw new NotSupportedException(CoreStrings.TransactionsNotSupported);
        }
    }
}
