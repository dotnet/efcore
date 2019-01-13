// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Defines different strategies for when cascading actions will be performed.
    ///     See <see cref="ChangeTracker.CascadeDeleteTiming" /> and <see cref="ChangeTracker.DeleteOrphansTiming" />.
    /// </summary>
    public enum CascadeTiming
    {
        /// <summary>
        ///     Cascading actions are made to dependent/child entities as soon as the principal/parent
        ///     entity changes.
        /// </summary>
        Immediate,

        /// <summary>
        ///     Cascading actions are made to dependent/child entities as part of <see cref="DbContext.SaveChanges()" />.
        /// </summary>
        OnSaveChanges,

        /// <summary>
        ///     Cascading actions are never made automatically to dependent/child entities, but must instead
        ///     be triggered by an explicit call.
        /// </summary>
        Never
    }
}
