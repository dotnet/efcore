// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking;

/// <summary>
///     Provides access to change tracking information and operations for a node in a
///     graph of entities that is being traversed.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-track-graph">Tracking entities in EF Core</see> for more information and examples.
/// </remarks>
public class EntityEntryGraphNode<TState> : EntityEntryGraphNode
{
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
        TState state,
        InternalEntityEntry? sourceEntry,
        INavigationBase? inboundNavigation)
        : base(entry, sourceEntry, inboundNavigation)
    {
        NodeState = state;
    }

    /// <summary>
    ///     Creates a new node in the entity graph.
    /// </summary>
    /// <param name="entry">The entry for the entity represented by this node.</param>
    /// <param name="state">A state object that will be available when processing each node.</param>
    /// <param name="sourceEntry">The entry from which this node was reached, or <see langword="null" /> if this is the root node.</param>
    /// <param name="inboundNavigation">The navigation from the source node to this node, or <see langword="null" /> if this is the root node.</param>
    public EntityEntryGraphNode(
        EntityEntry entry,
        TState state,
        EntityEntry? sourceEntry,
        INavigationBase? inboundNavigation)
        : this(entry.GetInfrastructure(), state, sourceEntry?.GetInfrastructure(), inboundNavigation)
    {
    }

    /// <summary>
    ///     Gets or sets state that will be available to all nodes that are visited after this node.
    /// </summary>
    public virtual TState NodeState { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public override EntityEntryGraphNode CreateNode(
        EntityEntryGraphNode currentNode,
        InternalEntityEntry internalEntityEntry,
        INavigationBase reachedVia)
        => new EntityEntryGraphNode<TState>(
            internalEntityEntry,
            ((EntityEntryGraphNode<TState>)currentNode).NodeState,
            currentNode.Entry.GetInfrastructure(),
            reachedVia);
}
