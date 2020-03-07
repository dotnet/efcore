// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
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
        public static DbTransaction GetDbTransaction([NotNull] this IDbContextTransaction dbContextTransaction)
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
