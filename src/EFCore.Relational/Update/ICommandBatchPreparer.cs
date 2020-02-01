// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

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
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface ICommandBatchPreparer
    {
        /// <summary>
        ///     Creates the command batches needed to insert/update/delete the entities represented by the given
        ///     list of <see cref="IUpdateEntry" />s.
        /// </summary>
        /// <param name="entries"> The entries that represent the entities to be modified. </param>
        /// <param name="updateAdapter"> The model data. </param>
        /// <returns> The list of batches to execute. </returns>
        IEnumerable<ModificationCommandBatch> BatchCommands(
            [NotNull] IList<IUpdateEntry> entries,
            [NotNull] IUpdateAdapter updateAdapter);
    }
}
