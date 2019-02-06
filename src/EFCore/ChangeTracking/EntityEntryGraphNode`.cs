// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Provides access to change tracking information and operations for a node in a
    ///     graph of entities that is being traversed.
    /// </summary>
    public class EntityEntryGraphNode<TState> : EntityEntryGraphNode
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [DebuggerStepThrough]
        public EntityEntryGraphNode(
            [NotNull] InternalEntityEntry entry,
            [CanBeNull] TState state,
            [CanBeNull] InternalEntityEntry sourceEntry,
            [CanBeNull] INavigation inboundNavigation)
            : base(entry, sourceEntry, inboundNavigation)
        {
            NodeState = state;
        }

        /// <summary>
        ///     Gets or sets state that will be available to all nodes that are visited after this node.
        /// </summary>
        public virtual TState NodeState { get; }

        /// <summary>
        ///     Creates a new node for the entity that is being traversed next in the graph.
        /// </summary>
        /// <param name="currentNode"> The node that the entity is being traversed from. </param>
        /// <param name="internalEntityEntry">
        ///     The internal entry tracking information about the entity being traversed to.
        /// </param>
        /// <param name="reachedVia"> The navigation property that is being traversed to reach the new node. </param>
        /// <returns> The newly created node. </returns>
        public override EntityEntryGraphNode CreateNode(
            EntityEntryGraphNode currentNode,
            InternalEntityEntry internalEntityEntry,
            INavigation reachedVia)
        {
            Check.NotNull(currentNode, nameof(currentNode));
            Check.NotNull(internalEntityEntry, nameof(internalEntityEntry));
            Check.NotNull(reachedVia, nameof(reachedVia));

            return new EntityEntryGraphNode<TState>(
                internalEntityEntry,
                ((EntityEntryGraphNode<TState>)currentNode).NodeState,
                currentNode.Entry.GetInfrastructure(),
                reachedVia);
        }
    }
}
