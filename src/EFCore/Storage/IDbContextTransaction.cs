// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         A transaction against the database.
    ///     </para>
    ///     <para>
    ///         Instances of this class are typically obtained from <see cref="DatabaseFacade.BeginTransaction" /> and it is not designed
    ///         to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public interface IDbContextTransaction : IDisposable, IAsyncDisposable
    {
        /// <summary>
        ///     Gets the transaction identifier.
        /// </summary>
        Guid TransactionId { get; }

        /// <summary>
        ///     Commits all changes made to the database in the current transaction.
        /// </summary>
        void Commit();

        /// <summary>
        ///     Commits all changes made to the database in the current transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Discards all changes made to the database in the current transaction.
        /// </summary>
        void Rollback();

        /// <summary>
        ///     Discards all changes made to the database in the current transaction asynchronously.
        /// </summary>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        Task RollbackAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Creates a savepoint in the transaction. This allows all commands that are executed after the savepoint
        ///     was established to be rolled back, restoring the transaction state to what it was at the time of the
        ///     savepoint.
        /// </summary>
        /// <param name="savepointName"> The name of the savepoint to be created. </param>
        void Save([NotNull] string savepointName) => throw new NotSupportedException();

        /// <summary>
        ///     Creates a savepoint in the transaction. This allows all commands that are executed after the savepoint
        ///     was established to be rolled back, restoring the transaction state to what it was at the time of the
        ///     savepoint.
        /// </summary>
        /// <param name="savepointName"> The name of the savepoint to be created. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        Task SaveAsync([NotNull] string savepointName, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        /// <summary>
        ///     Rolls back all commands that were executed after the specified savepoint was established.
        /// </summary>
        /// <param name="savepointName"> The name of the savepoint to roll back to. </param>
        void Rollback([NotNull] string savepointName) => throw new NotSupportedException();

        /// <summary>
        ///     Rolls back all commands that were executed after the specified savepoint was established.
        /// </summary>
        /// <param name="savepointName"> The name of the savepoint to roll back to. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        Task RollbackAsync([NotNull] string savepointName, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        /// <summary>
        ///     Destroys a savepoint previously defined in the current transaction. This allows the system to
        ///     reclaim some resources before the transaction ends.
        /// </summary>
        /// <param name="savepointName"> The name of the savepoint to release. </param>
        void Release([NotNull] string savepointName) { }

        /// <summary>
        ///     Destroys a savepoint previously defined in the current transaction. This allows the system to
        ///     reclaim some resources before the transaction ends.
        /// </summary>
        /// <param name="savepointName"> The name of the savepoint to release. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        Task ReleaseAsync([NotNull] string savepointName, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        /// <summary>
        ///     Gets a value that indicates whether this <see cref="IDbContextTransaction"/> instance supports
        ///     database savepoints. If <c>false</c>, the methods <see cref="SaveAsync"/>,
        ///     <see cref="RollbackAsync(string, System.Threading.CancellationToken)"/>
        ///     and <see cref="ReleaseAsync"/> as well as their synchronous counterparts are expected to throw
        ///     <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if this <see cref="IDbContextTransaction"/> instance supports database savepoints;
        ///     otherwise, <c>false</c>.
        /// </returns>
        bool AreSavepointsSupported => false;
    }
}
