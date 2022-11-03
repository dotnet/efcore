// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Event arguments for the <see cref="ChangeTracker.Tracked" /> event.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-state-changes">State changes of entities in EF Core</see> for more information and examples.
/// </remarks>
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
        InternalEntityEntry internalEntityEntry,
        bool fromQuery)
        : base(internalEntityEntry)
    {
        FromQuery = fromQuery;
    }

    /// <summary>
    ///     <see langword="true" /> if the entity is being tracked as part of a database query; <see langword="false" /> otherwise.
    /// </summary>
    public virtual bool FromQuery { get; }
}
