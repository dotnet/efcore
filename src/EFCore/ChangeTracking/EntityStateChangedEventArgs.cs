// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Event arguments for the <see cref="ChangeTracker.StateChanged" /> event.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-state-changes">State changes of entities in EF Core</see> for more information and examples.
/// </remarks>
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
        InternalEntityEntry internalEntityEntry,
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
