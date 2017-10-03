// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    ///     A service for executing migration commands against a database.
    /// </summary>
    public interface IMigrationCommandExecutor
    {
        /// <summary>
        ///     Executes the given commands using the given database connection.
        /// </summary>
        /// <param name="migrationCommands"> The commands to execute. </param>
        /// <param name="connection"> The connection to use. </param>
        void ExecuteNonQuery(
            [NotNull] IEnumerable<MigrationCommand> migrationCommands,
            [NotNull] IRelationalConnection connection);

        /// <summary>
        ///     Executes the given commands using the given database connection.
        /// </summary>
        /// <param name="migrationCommands"> The commands to execute. </param>
        /// <param name="connection"> The connection to use. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns> A task that represents the asynchronous operation. </returns>
        Task ExecuteNonQueryAsync(
            [NotNull] IEnumerable<MigrationCommand> migrationCommands,
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default);
    }
}
