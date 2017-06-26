// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Creates and deletes schemas for a given schema provider.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by schema providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface ISchemaCreator
    {
        /// <summary>
        ///     <para>
        ///         Ensures that the schema for the context does not exist. If it does not exist, no action is taken. If it does
        ///         exist then the schema is deleted.
        ///     </para>
        /// </summary>
        /// <returns> True if the schema is deleted, false if it did not exist. </returns>
        bool EnsureDeleted();

        /// <summary>
        ///     <para>
        ///         Asynchronously ensures that the schema for the context does not exist. If it does not exist, no action is taken. If it does
        ///         exist then the schema is deleted.
        ///     </para>
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains true if the schema is deleted,
        ///     false if it did not exist.
        /// </returns>
        Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        ///     Ensures that the schema for the context exists. If it exists, no action is taken. If it does not
        ///     exist then the schema is created. If the schema exists, then no effort is made
        ///     to ensure it is compatible with the model for this context.
        /// </summary>
        /// <returns> True if the schema is created, false if it already existed. </returns>
        bool EnsureCreated();

        /// <summary>
        ///     Asynchronously ensures that the schema for the context exists. If it exists, no action is taken. If it does not
        ///     exist then the schema is created. If the schema exists, then no effort is made
        ///     to ensure it is compatible with the model for this context.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains true if the schema is created,
        ///     false if it already existed.
        /// </returns>
        Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}