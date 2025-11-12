// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     Represents an exclusive lock on the database that is used to ensure that only one migration application can be run at a time.
/// </summary>
/// <remarks>
///     Typically only database providers implement this.
/// </remarks>
public interface IMigrationsDatabaseLock : IDisposable, IAsyncDisposable
{
    /// <summary>
    ///    The history repository.
    /// </summary>
    protected IHistoryRepository HistoryRepository { get; }

    /// <summary>
    ///     Acquires an exclusive lock on the database again if the current one was already released.
    /// </summary>
    /// <param name="connectionReopened">Indicates whether the connection was reopened.</param>
    /// <param name="transactionRestarted">
    ///     Indicates whether the transaction was restarted.
    ///     <see langword="null"/> if there's no current transaction.
    /// </param>
    /// <returns>An object that can be disposed to release the lock.</returns>
    IMigrationsDatabaseLock ReacquireIfNeeded(bool connectionReopened, bool? transactionRestarted)
    {
        if ((connectionReopened && HistoryRepository.LockReleaseBehavior == LockReleaseBehavior.Connection)
                || (transactionRestarted is true && HistoryRepository.LockReleaseBehavior == LockReleaseBehavior.Transaction))
        {
            Dispose();
            return HistoryRepository.AcquireDatabaseLock();
        }

        return this;
    }

    /// <summary>
    ///     Acquires an exclusive lock on the database again, if the current one was already released.
    /// </summary>
    /// <param name="connectionReopened">Indicates whether the connection was reopened.</param>
    /// <param name="transactionRestarted">
    ///     Indicates whether the transaction was restarted.
    ///     <see langword="null"/> if there's no current transaction.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>An object that can be disposed to release the lock.</returns>
    async Task<IMigrationsDatabaseLock> ReacquireIfNeededAsync(
        bool connectionReopened, bool? transactionRestarted, CancellationToken cancellationToken = default)
    {
        if ((connectionReopened && HistoryRepository.LockReleaseBehavior == LockReleaseBehavior.Connection)
                || (transactionRestarted is true && HistoryRepository.LockReleaseBehavior == LockReleaseBehavior.Transaction))
        {
            await DisposeAsync().ConfigureAwait(false);
            return await HistoryRepository.AcquireDatabaseLockAsync(cancellationToken).ConfigureAwait(false);
        }

        return this;
    }
}
