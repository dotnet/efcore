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
        /// <param name="name"> The name of the savepoint to be created. </param>
        void CreateSavepoint([NotNull] string name)
            => throw new NotSupportedException();

        /// <summary>
        ///     Creates a savepoint in the transaction. This allows all commands that are executed after the savepoint
        ///     was established to be rolled back, restoring the transaction state to what it was at the time of the
        ///     savepoint.
        /// </summary>
        /// <param name="name"> The name of the savepoint to be created. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        Task CreateSavepointAsync([NotNull] string name, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        /// <summary>
        ///     Rolls back all commands that were executed after the specified savepoint was established.
        /// </summary>
        /// <param name="name"> The name of the savepoint to roll back to. </param>
        void RollbackToSavepoint([NotNull] string name)
            => throw new NotSupportedException();

        /// <summary>
        ///     Rolls back all commands that were executed after the specified savepoint was established.
        /// </summary>
        /// <param name="name"> The name of the savepoint to roll back to. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        Task RollbackToSavepointAsync([NotNull] string name, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        /// <summary>
        ///     Destroys a savepoint previously defined in the current transaction. This allows the system to
        ///     reclaim some resources before the transaction ends.
        /// </summary>
        /// <param name="name"> The name of the savepoint to release. </param>
        void ReleaseSavepoint([NotNull] string name) { }

        /// <summary>
        ///     Destroys a savepoint previously defined in the current transaction. This allows the system to
        ///     reclaim some resources before the transaction ends.
        /// </summary>
        /// <param name="name"> The name of the savepoint to release. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        Task ReleaseSavepointAsync([NotNull] string name, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        /// <summary>
        ///     Gets a value that indicates whether this <see cref="IDbContextTransaction" /> instance supports
        ///     database savepoints. If <see langword="false" />, the methods <see cref="CreateSavepointAsync" />,
        ///     <see cref="RollbackToSavepointAsync" />
        ///     and <see cref="ReleaseSavepointAsync" /> as well as their synchronous counterparts are expected to throw
        ///     <see cref="NotSupportedException" />.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if this <see cref="IDbContextTransaction" /> instance supports database savepoints;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        bool SupportsSavepoints
            => false;
    }
}
