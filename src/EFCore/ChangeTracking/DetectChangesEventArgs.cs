// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Event arguments for the <see cref="ChangeTracker.DetectingChanges" /> event.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-state-changes">State changes of entities in EF Core</see> for more information and examples.
/// </remarks>
public class DetectChangesEventArgs : EventArgs
{
    private readonly InternalEntityEntry? _internalEntityEntry;
    private EntityEntry? _entry;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public DetectChangesEventArgs(InternalEntityEntry? internalEntityEntry)
    {
        _internalEntityEntry = internalEntityEntry;
    }

    /// <summary>
    ///     If detecting changes for a single entity, then this is the <see cref="EntityEntry" /> for that entity.
    ///     If detecting changes for an entire graph, then <see langword="null" />.
    /// </summary>
    public virtual EntityEntry? Entry
        => _internalEntityEntry == null
            ? null
            : (_entry ??= new EntityEntry(_internalEntityEntry));
}
