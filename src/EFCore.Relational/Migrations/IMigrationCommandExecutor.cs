// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     <para>
    ///         A service for executing migration commands against a database.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface IMigrationCommandExecutor
    {
        /// <summary>
        ///     Executes the given commands using the given database connection.
        /// </summary>
        /// <param name="migrationCommands"> The commands to execute. </param>
        /// <param name="connection"> The connection to use. </param>
        void ExecuteNonQuery(
            IEnumerable<MigrationCommand> migrationCommands,
            IRelationalConnection connection);

        /// <summary>
        ///     Executes the given commands using the given database connection.
        /// </summary>
        /// <param name="migrationCommands"> The commands to execute. </param>
        /// <param name="connection"> The connection to use. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        Task ExecuteNonQueryAsync(
            IEnumerable<MigrationCommand> migrationCommands,
            IRelationalConnection connection,
            CancellationToken cancellationToken = default);
    }
}
