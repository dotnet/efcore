// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Factory for creating <see cref="IUpdateAdapter" /> instances.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IUpdateAdapterFactory
    {
        /// <summary>
        ///     Creates a tracker for the model currently in use.
        /// </summary>
        /// <returns> The new tracker. </returns>
        IUpdateAdapter Create();

        /// <summary>
        ///     <para>
        ///         Creates a standalone tracker that works with its own <see cref="IStateManager"/> and hence will not
        ///         impact tracking on the state manager currently in use.
        ///     </para>
        ///     <para>
        ///         The <see cref="IUpdateAdapter.Entries" /> from this update adapter should be used explicitly
        ///         once they have been setup. They will not be visible to other parts of the stack,
        ///         including <see cref="DbContext.SaveChanges()" />.
        ///     </para>
        /// </summary>
        /// <param name="model"> The model for which a tracker is needed, or null to use the current model. </param>
        /// <returns> The new tracker. </returns>
        IUpdateAdapter CreateStandalone([CanBeNull] IModel model = null);
    }
}
