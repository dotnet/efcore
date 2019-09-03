// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A service for executing one or more batches of insert/update/delete commands against a database.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IBatchExecutor
    {
        /// <summary>
        ///     Executes the commands in the batches against the given database connection.
        /// </summary>
        /// <param name="commandBatches"> The batches to execute. </param>
        /// <param name="connection"> The database connection to use. </param>
        /// <returns> The total number of rows affected. </returns>
        int Execute(
            [NotNull] IEnumerable<ModificationCommandBatch> commandBatches,
            [NotNull] IRelationalConnection connection);

        /// <summary>
        ///     Executes the commands in the batches against the given database connection.
        /// </summary>
        /// <param name="commandBatches"> The batches to execute. </param>
        /// <param name="connection"> The database connection to use. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns>
        ///     A task that represents the asynchronous save operation. The task result contains the
        ///     total number of rows affected.
        /// </returns>
        Task<int> ExecuteAsync(
            [NotNull] IEnumerable<ModificationCommandBatch> commandBatches,
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default);
    }
}
