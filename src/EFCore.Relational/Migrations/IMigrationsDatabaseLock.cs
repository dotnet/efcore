// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     Represents an exclusive lock on the database that is used to ensure that only one migration application can be run at a time.
/// </summary>
/// <remarks>
///     Database providers typically implement this.
/// </remarks>
public interface IMigrationsDatabaseLock : IDisposable, IAsyncDisposable
{
    /// <summary>
    ///     Acquires an exclusive lock on the database again, if the current one was already released.
    /// </summary>
    /// <param name="transaction">The transaction currently in use.</param>
    /// <returns>An object that can be disposed to release the lock.</returns>
    IMigrationsDatabaseLock Reacquire(IDbContextTransaction? transaction);

    /// <summary>
    ///     Acquires an exclusive lock on the database again, if the current one was already released.
    /// </summary>
    /// <param name="transaction">The transaction currently in use.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>An object that can be disposed to release the lock.</returns>
    Task<IMigrationsDatabaseLock> ReacquireAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken = default);
}
