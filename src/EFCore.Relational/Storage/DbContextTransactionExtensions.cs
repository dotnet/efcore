// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Extension methods for <see cref="IDbContextTransaction" />.
    /// </summary>
    public static class DbContextTransactionExtensions
    {
        /// <summary>
        ///     Gets the underlying <see cref="DbTransaction" /> for the given transaction. Throws if the database being targeted
        ///     is not a relational database that uses <see cref="DbTransaction" />.
        /// </summary>
        /// <param name="dbContextTransaction"> The transaction to get the <see cref="DbTransaction" /> from. </param>
        /// <returns> The underlying <see cref="DbTransaction" />. </returns>
        public static DbTransaction GetDbTransaction(this IDbContextTransaction dbContextTransaction)
        {
            Check.NotNull(dbContextTransaction, nameof(dbContextTransaction));

            if (!(dbContextTransaction is IInfrastructure<DbTransaction> accessor))
            {
                throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
            }

            return accessor.GetInfrastructure();
        }
    }
}
