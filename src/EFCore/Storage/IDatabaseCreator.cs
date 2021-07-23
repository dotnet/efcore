// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Creates and deletes databases for a given database provider.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IDatabaseCreator
    {
        /// <summary>
        ///     <para>
        ///         Ensures that the database for the context does not exist. If it does not exist, no action is taken. If it does
        ///         exist then the database is deleted.
        ///     </para>
        ///     <para>
        ///         Warning: The entire database is deleted an no effort is made to remove just the database objects that are used by
        ///         the model for this context.
        ///     </para>
        /// </summary>
        /// <returns> <see langword="true" /> if the database is deleted, <see langword="false" /> if it did not exist. </returns>
        bool EnsureDeleted();

        /// <summary>
        ///     <para>
        ///         Asynchronously ensures that the database for the context does not exist. If it does not exist, no action is taken. If it does
        ///         exist then the database is deleted.
        ///     </para>
        ///     <para>
        ///         Warning: The entire database is deleted an no effort is made to remove just the database objects that are used by
        ///         the model for this context.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains <see langword="true" /> if the database is deleted,
        ///     <see langword="false" /> if it did not exist.
        /// </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     Ensures that the database for the context exists. If it exists, no action is taken. If it does not
        ///     exist then the database and all its schema are created. If the database exists, then no effort is made
        ///     to ensure it is compatible with the model for this context.
        /// </summary>
        /// <returns> <see langword="true" /> if the database is created, <see langword="false" /> if it already existed. </returns>
        bool EnsureCreated();

        /// <summary>
        ///     Asynchronously ensures that the database for the context exists. If it exists, no action is taken. If it does not
        ///     exist then the database and all its schema are created. If the database exists, then no effort is made
        ///     to ensure it is compatible with the model for this context.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains <see langword="true" /> if the database is created,
        ///     <see langword="false" /> if it already existed.
        /// </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        ///     <para>
        ///         Determines whether or not the database is available and can be connected to.
        ///     </para>
        ///     <para>
        ///         Note that being able to connect to the database does not mean that it is
        ///         up-to-date with regard to schema creation, etc.
        ///     </para>
        /// </summary>
        /// <returns> <see langword="true" /> if the database is available; <see langword="false" /> otherwise. </returns>
        bool CanConnect();

        /// <summary>
        ///     <para>
        ///         Determines whether or not the database is available and can be connected to.
        ///     </para>
        ///     <para>
        ///         Note that being able to connect to the database does not mean that it is
        ///         up-to-date with regard to schema creation, etc.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns> <see langword="true" /> if the database is available; <see langword="false" /> otherwise. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
    }
}
