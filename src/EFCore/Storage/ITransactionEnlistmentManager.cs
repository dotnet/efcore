// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Transactions;

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         Manages the current <see cref="Transaction" />.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public interface ITransactionEnlistmentManager
{
    /// <summary>
    ///     The current ambient transaction. Defaults to <see cref="Transaction.Current" />.
    /// </summary>
    Transaction? CurrentAmbientTransaction
        => Transaction.Current;

    /// <summary>
    ///     The currently enlisted transaction.
    /// </summary>
    Transaction? EnlistedTransaction { get; }

    /// <summary>
    ///     Specifies an existing <see cref="Transaction" /> to be used for database operations.
    /// </summary>
    /// <param name="transaction">The transaction to be used.</param>
    void EnlistTransaction(Transaction? transaction);
}
