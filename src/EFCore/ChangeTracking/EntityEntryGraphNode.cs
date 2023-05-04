// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Provides access to change tracking information and operations for a node in a
///     graph of entities that is being traversed.
/// </summary>
/// <remarks>
///     <para>
///         See <see cref="ChangeTracker.TrackGraph" /> for information on how graph nodes are used.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-track-graph">Tracking entities in EF Core</see> for more information and examples.
///     </para>
/// </remarks>
public class EntityEntryGraphNode : IInfrastructure<InternalEntityEntry>
{
    private readonly InternalEntityEntry _entry;
    private readonly InternalEntityEntry? _sourceEntry;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [DebuggerStepThrough]
    [EntityFrameworkInternal]
    public EntityEntryGraphNode(
        InternalEntityEntry entry,
        InternalEntityEntry? sourceEntry,
        INavigationBase? inboundNavigation)
    {
        _entry = entry;
        _sourceEntry = sourceEntry;
        InboundNavigation = inboundNavigation;
    }

    /// <summary>
    ///     An <see cref="EntityEntry" /> for the entity instance from which a navigation property was traversed to the instance
    ///     represented by this node.
    /// </summary>
    /// <remarks>
    ///     See <see cref="ChangeTracker.TrackGraph" /> for information on how graph nodes are used.
    /// </remarks>
    public virtual EntityEntry? SourceEntry
        => _sourceEntry == null ? null : new EntityEntry(_sourceEntry);

    /// <summary>
    ///     Gets the navigation property that is being traversed to reach this node in the graph.
    /// </summary>
    /// <remarks>
    ///     See <see cref="ChangeTracker.TrackGraph" /> for information on how graph nodes are used.
    /// </remarks>
    public virtual INavigationBase? InboundNavigation { get; }

    /// <summary>
    ///     An <see cref="EntityEntry" /> for the entity instance represented by this node.
    /// </summary>
    /// <remarks>
    ///     See <see cref="ChangeTracker.TrackGraph" /> for information on how graph nodes are used.
    /// </remarks>
    public virtual EntityEntry Entry
        => new(_entry);

    /// <summary>
    ///     <para>
    ///         Gets the internal entry that is tracking information about this entity.
    ///     </para>
    ///     <para>
    ///         This property is intended for use by extension methods. It is not intended to be used in
    ///         application code.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    /// </remarks>
    [EntityFrameworkInternal]
    InternalEntityEntry IInfrastructure<InternalEntityEntry>.Instance
        => _entry;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public virtual EntityEntryGraphNode CreateNode(
        EntityEntryGraphNode currentNode,
        InternalEntityEntry internalEntityEntry,
        INavigationBase reachedVia)
        => new(internalEntityEntry, currentNode.Entry.GetInfrastructure(), reachedVia);
}
