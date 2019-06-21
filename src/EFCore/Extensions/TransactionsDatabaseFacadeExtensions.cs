// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
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
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context.</param>
        /// <param name="transaction"> The transaction to be used. </param>
        public static void EnlistTransaction([NotNull] this DatabaseFacade databaseFacade, [CanBeNull] Transaction transaction)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));
            if (((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies.TransactionManager is ITransactionEnlistmentManager transactionManager)
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
        /// <param name="databaseFacade"> The <see cref="DatabaseFacade" /> for the context.</param>
        /// <returns> The currently enlisted transaction. </returns>
        public static Transaction GetEnlistedTransaction([NotNull] this DatabaseFacade databaseFacade)
        {
            Check.NotNull(databaseFacade, nameof(databaseFacade));
            if (((IDatabaseFacadeDependenciesAccessor)databaseFacade).Dependencies.TransactionManager is ITransactionEnlistmentManager transactionManager)
            {
                return transactionManager.EnlistedTransaction;
            }

            throw new NotSupportedException(CoreStrings.TransactionsNotSupported);
        }
    }
}
