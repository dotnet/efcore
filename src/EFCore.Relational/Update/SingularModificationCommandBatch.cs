// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         An implementation of <see cref="AffectedCountModificationCommandBatch" /> that does not
    ///         support batching by limiting the number of commands in the batch to one.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public class SingularModificationCommandBatch : AffectedCountModificationCommandBatch
    {
        /// <summary>
        ///     Creates a new <see cref="SingularModificationCommandBatch" /> instance.
        /// </summary>
        /// <param name="dependencies"> Service dependencies. </param>
        public SingularModificationCommandBatch([NotNull] ModificationCommandBatchFactoryDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     Only returns <see langword="true" /> if the no command has already been added.
        /// </summary>
        /// <param name="modificationCommand"> The command to potentially add. </param>
        /// <returns> <see langword="true" /> if no command has already been added. </returns>
        protected override bool CanAddCommand(ModificationCommand modificationCommand)
            => ModificationCommands.Count == 0;

        /// <summary>
        ///     Returns <see langword="true" /> since only a single command is generated so the command text must be valid.
        /// </summary>
        /// <returns>
        ///     <see langword="true" />
        /// </returns>
        protected override bool IsCommandTextValid()
            => true;
    }
}
