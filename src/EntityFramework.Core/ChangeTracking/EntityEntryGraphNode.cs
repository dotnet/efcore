// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    /// <summary>
    ///     Provides access to change tracking information and operations for a node in a 
    ///     graph of entities that is being traversed.
    /// </summary>
    public class EntityEntryGraphNode : EntityGraphNodeBase<EntityEntryGraphNode>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="EntityGraphNodeBase{TNode}" /> class.
        /// </summary>
        /// <param name="context"> The context that the graph traversal was initiated from. </param>
        /// <param name="internalEntityEntry"> The internal entry tracking information about this entity. </param>
        /// <param name="inboundNavigation"> 
        ///     The navigation property that is being traversed to reach this node in the graph.
        /// </param>
        public EntityEntryGraphNode(
            [NotNull] DbContext context,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [CanBeNull] INavigation inboundNavigation)
            : base(internalEntityEntry, inboundNavigation)
        {
            Check.NotNull(context, nameof(context));

            Context = context;
            Entry = new EntityEntry(context, internalEntityEntry);
        }

        /// <summary>
        ///     Gets the context that the graph traversal was initiated from.
        /// </summary>
        public virtual DbContext Context { get; }

        /// <summary>
        ///     Gets the entry tracking information about this entity.
        /// </summary>
        public new virtual EntityEntry Entry { get; }

        /// <summary>
        ///     Creates a new node for the entity that is being traversed next in the graph.
        /// </summary>
        /// <param name="currentNode"> The node that the entity is being traversed from. </param>
        /// <param name="internalEntityEntry">
        ///      The internal entry tracking information about the entity being traversed to. 
        /// </param>
        /// <param name="reachedVia"> The navigation property that is being traversed to reach the new node. </param>
        /// <returns> The newly created node. </returns>
        public override EntityEntryGraphNode CreateNode(
            EntityEntryGraphNode currentNode,
            InternalEntityEntry internalEntityEntry,
            INavigation reachedVia)
            => new EntityEntryGraphNode(
                Context,
                Check.NotNull(internalEntityEntry, nameof(internalEntityEntry)),
                Check.NotNull(reachedVia, nameof(reachedVia)))
                {
                    NodeState = Check.NotNull(currentNode, nameof(currentNode)).NodeState
                };
    }
}
