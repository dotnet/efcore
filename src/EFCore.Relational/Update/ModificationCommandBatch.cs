// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
        public abstract IReadOnlyList<ModificationCommand> ModificationCommands { get; }

        /// <summary>
        ///     Adds the given insert/update/delete <see cref="ModificationCommands" /> to the batch.
        /// </summary>
        /// <param name="modificationCommand"> The command to add. </param>
        /// <returns>
        ///     <c>True</c> if the command was successfully added; <c>false</c> if there was no
        ///     room in the current batch to add the command and it must instead be added to a new batch.
        /// </returns>
        public abstract bool AddCommand([NotNull] ModificationCommand modificationCommand);

        /// <summary>
        ///     Sends insert/update/delete commands to the database.
        /// </summary>
        /// <param name="connection"> The database connection to use. </param>
        public abstract void Execute([NotNull] IRelationalConnection connection);

        /// <summary>
        ///     Sends insert/update/delete commands to the database.
        /// </summary>
        /// <param name="connection"> The database connection to use. </param>
        /// <param name="cancellationToken">A <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
        /// <returns> A task that represents the asynchronous save operation. </returns>
        public abstract Task ExecuteAsync(
            [NotNull] IRelationalConnection connection,
            CancellationToken cancellationToken = default);
    }
}
