// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A service for preparing a list of <see cref="ModificationCommandBatch" />s for the entities
    ///         represented by the given list of <see cref="IUpdateEntry" />s.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public interface ICommandBatchPreparer
    {
        /// <summary>
        ///     Creates the command batches needed to insert/update/delete the entities represented by the given
        ///     list of <see cref="IUpdateEntry" />s.
        /// </summary>
        /// <param name="entries"> The entries that represent the entities to be modified. </param>
        /// <returns> The list of batches to execute. </returns>
        IEnumerable<ModificationCommandBatch> BatchCommands([NotNull] IReadOnlyList<IUpdateEntry> entries);
    }
}
