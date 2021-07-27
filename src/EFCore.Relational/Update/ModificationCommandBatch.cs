// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A base class for a collection of <see cref="ModificationCommand" />s that can be executed
    ///         as a batch.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public abstract class ModificationCommandBatch
    {
        /// <summary>
        ///     The list of conceptual insert/update/delete <see cref="ModificationCommands" />s in the batch.
        /// </summary>
        public abstract IReadOnlyList<IModificationCommand> ModificationCommands { get; }

        /// <summary>
        ///     Adds the given insert/update/delete <see cref="ModificationCommands" /> to the batch.
        /// </summary>
        /// <param name="modificationCommand"> The command to add. </param>
        /// <returns>
        ///     <see langword="true" /> if the command was successfully added; <see langword="false" /> if there was no
        ///     room in the current batch to add the command and it must instead be added to a new batch.
        /// </returns>
        public abstract bool AddCommand(IModificationCommand modificationCommand);

        /// <summary>
        ///     Sends insert/update/delete commands to the database.
        /// </summary>
        /// <param name="connection"> The database connection to use. </param>
        public abstract void Execute(IRelationalConnection connection);

        /// <summary>
        ///     Sends insert/update/delete commands to the database.
        /// </summary>
        /// <param name="connection"> The database connection to use. </param>
        /// <param name="cancellationToken"> A <see cref="CancellationToken" /> to observe while waiting for the task to complete. </param>
        /// <returns> A task that represents the asynchronous save operation. </returns>
        /// <exception cref="OperationCanceledException"> If the <see cref="CancellationToken"/> is canceled. </exception>
        public abstract Task ExecuteAsync(
            IRelationalConnection connection,
            CancellationToken cancellationToken = default);
    }
}
