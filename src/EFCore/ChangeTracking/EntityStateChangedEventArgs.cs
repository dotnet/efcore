// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Event arguments for the <see cref="ChangeTracker.StateChanged" /> event.
    /// </summary>
    public class EntityStateChangedEventArgs : EntityEntryEventArgs
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
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
