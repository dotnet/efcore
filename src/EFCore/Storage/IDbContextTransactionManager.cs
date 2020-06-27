// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Creates and manages the current transaction.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IDbContextTransactionManager : IResettableService
    {
        /// <summary>
        ///     Begins a new transaction.
        /// </summary>
        /// <returns> The newly created transaction. </returns>
        IDbContextTransaction BeginTransaction();

        /// <summary>
        ///     Asynchronously begins a new transaction.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains the newly created transaction.
        /// </returns>
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Commits all changes made to the database in the current transaction.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        ///     Commits all changes made to the database in the current transaction.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Discards all changes made to the database in the current transaction.
        /// </summary>
        void RollbackTransaction();

        /// <summary>
        ///     Discards all changes made to the database in the current transaction.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous operation.
        /// </returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Creates a savepoint in the transaction. This allows all commands that are executed after the savepoint
        ///     was established to be rolled back, restoring the transaction state to what it was at the time of the
        ///     savepoint.
        /// </summary>
        /// <param name="name"> The name of the savepoint to be created. </param>
        void CreateSavepoint([NotNull] string name) => throw new NotSupportedException();

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
        void RollbackToSavepoint([NotNull] string name) => throw new NotSupportedException();

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
        void ReleaseSavepoint([NotNull] string name) => throw new NotSupportedException();

        /// <summary>
        ///     Destroys a savepoint previously defined in the current transaction. This allows the system to
        ///     reclaim some resources before the transaction ends.
        /// </summary>
        /// <param name="name"> The name of the savepoint to release. </param>
        /// <param name="cancellationToken"> The cancellation token. </param>
        /// <returns> A <see cref="Task" /> representing the asynchronous operation. </returns>
        Task ReleaseSavepointAsync([NotNull] string name, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        /// <summary>
        ///     Gets a value that indicates whether this <see cref="IDbContextTransactionManager"/> instance supports
        ///     database savepoints. If <see langword="false" />, the methods <see cref="CreateSavepointAsync"/>,
        ///     <see cref="RollbackToSavepointAsync"/>
        ///     and <see cref="ReleaseSavepointAsync"/> as well as their synchronous counterparts are expected to throw
        ///     <see cref="NotSupportedException"/>.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if this <see cref="IDbContextTransactionManager"/> instance supports database savepoints;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        bool AreSavepointsSupported => false;

        /// <summary>
        ///     Gets the current transaction.
        /// </summary>
        IDbContextTransaction CurrentTransaction { get; }
    }
}
