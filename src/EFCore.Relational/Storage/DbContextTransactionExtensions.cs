// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     Extension methods for <see cref="IDbContextTransaction" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
/// </remarks>
public static class DbContextTransactionExtensions
{
    /// <summary>
    ///     Gets the underlying <see cref="DbTransaction" /> for the given transaction. Throws if the database being targeted
    ///     is not a relational database that uses <see cref="DbTransaction" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-transactions">Transactions in EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="dbContextTransaction">The transaction to get the <see cref="DbTransaction" /> from.</param>
    /// <returns>The underlying <see cref="DbTransaction" />.</returns>
    public static DbTransaction GetDbTransaction(this IDbContextTransaction dbContextTransaction)
    {
        if (dbContextTransaction is not IInfrastructure<DbTransaction> accessor)
        {
            throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
        }

        return accessor.GetInfrastructure();
    }
}
