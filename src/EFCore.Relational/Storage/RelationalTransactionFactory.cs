// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;

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
    public class RelationalTransactionFactory : IRelationalTransactionFactory
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalTransactionFactory" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public RelationalTransactionFactory([NotNull] RelationalTransactionFactoryDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual RelationalTransactionFactoryDependencies Dependencies { get; }

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
        public virtual RelationalTransaction Create(
            IRelationalConnection connection,
            DbTransaction transaction,
            IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
            bool transactionOwned)
            => new RelationalTransaction(connection, transaction, logger, transactionOwned);
    }
}
