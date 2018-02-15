// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Event arguments for the <see cref="ChangeTracker.StateChanged" /> event.
    /// </summary>
    public class EntityStateChangedEventArgs : EntityEntryEventArgs
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityStateChangedEventArgs(
            [NotNull] InternalEntityEntry internalEntityEntry,
            EntityState oldState,
            EntityState newState)
            : base(internalEntityEntry)
        {
            OldState = oldState;
            NewState = newState;
        }

        /// <summary>
        ///     The state that the entity is transitioning from.
        /// </summary>
        public virtual EntityState OldState { get; }

        /// <summary>
        ///     The state that the entity is transitioning to.
        /// </summary>
        public virtual EntityState NewState { get; }
    }
}
