// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Event arguments for the <see cref="ChangeTracker.Tracked" /> event.
    /// </summary>
    public class EntityTrackedEventArgs : EntityEntryEventArgs
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
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
