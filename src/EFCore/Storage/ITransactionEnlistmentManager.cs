// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Transactions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Manages the current <see cref="Transaction" />.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface ITransactionEnlistmentManager
    {
        /// <summary>
        ///     The currently enlisted transaction.
        /// </summary>
        Transaction EnlistedTransaction { get; }

        /// <summary>
        ///     Specifies an existing <see cref="Transaction" /> to be used for database operations.
        /// </summary>
        /// <param name="transaction"> The transaction to be used. </param>
        void EnlistTransaction([CanBeNull] Transaction transaction);
    }
}
