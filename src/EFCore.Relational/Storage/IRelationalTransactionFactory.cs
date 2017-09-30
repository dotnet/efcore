// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         A factory for creating <see cref="RelationalTransaction" /> instances.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers It is generally not used in application code.
    ///     </para>
    /// </summary>
    public interface IRelationalTransactionFactory
    {
        /// <summary>
        ///     Creates a <see cref="RelationalTransaction" /> instance.
        /// </summary>
        /// <param name="connection"> The connection to the database. </param>
        /// <param name="transaction"> The underlying <see cref="DbTransaction" />. </param>
        /// <param name="logger"> The logger to write to. </param>
        /// <param name="transactionOwned">
        ///     A value indicating whether the transaction is owned by this class (i.e. if it can be disposed when this class is disposed).
        /// </param>
        /// <returns> A new <see cref="RelationalTransaction" /> instance. </returns>
        RelationalTransaction Create(
            [NotNull] IRelationalConnection connection,
            [NotNull] DbTransaction transaction,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned);
    }
}
