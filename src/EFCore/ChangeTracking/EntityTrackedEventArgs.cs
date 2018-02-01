// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Event arguments for the <see cref="ChangeTracker.Tracked" /> event.
    /// </summary>
    public class EntityTrackedEventArgs : EntityEntryEventArgs
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityTrackedEventArgs(
            [NotNull] InternalEntityEntry internalEntityEntry,
            bool fromQuery)
            : base(internalEntityEntry)
        {
            FromQuery = fromQuery;
        }

        /// <summary>
        ///     <c>True</c> if the entity is being tracked as part of a database query; <c>false</c> otherwise.
        /// </summary>
        public virtual bool FromQuery { get; }
    }
}
