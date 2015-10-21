// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Base implementation of a class that provides access to change tracking information and operations for a node in a 
    ///         graph of entities that is being traversed.
    ///     </para>
    ///     <para>
    ///         Instances of this class are created for you when using the
    ///         <see cref="ChangeTracker.TrackGraph(object, System.Action{EntityEntryGraphNode}, object)"/> API and it is
    ///         not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    /// <typeparam name="TNode"> The derived node type that is inheriting this class. </typeparam>
    public abstract class EntityGraphNodeBase<TNode>
        where TNode : EntityGraphNodeBase<TNode>
    {
        /// <summary>
        ///     <para>
        ///         Initializes a new instance of the <see cref="EntityGraphNodeBase{TNode}" /> class.
        ///     </para>
        ///     <para>
        ///         Instances of this class are created for you when using the
        ///         <see cref="ChangeTracker.TrackGraph(object, System.Action{EntityEntryGraphNode}, object)"/> API and it is
        ///         not designed to be directly constructed in your application code.
        ///     </para>
        /// </summary>
        /// <param name="internalEntityEntry"> The internal entry tracking information about this entity. </param>
        /// <param name="inboundNavigation"> 
        ///     The navigation property that is being traversed to reach this node in the graph.
        /// </param>
        protected EntityGraphNodeBase(
            [NotNull] InternalEntityEntry internalEntityEntry, 
            [CanBeNull] INavigation inboundNavigation)
        {
            Check.NotNull(internalEntityEntry, nameof(internalEntityEntry));

            Entry = internalEntityEntry;
            InboundNavigation = inboundNavigation;
        }

        /// <summary>
        ///     Gets the navigation property that is being traversed to reach this node in the graph.
        /// </summary>
        public virtual INavigation InboundNavigation { get; }

        /// <summary>
        ///     Gets the internal entry tracking information about this entity.
        /// </summary>
        public virtual InternalEntityEntry Entry { get; }

        /// <summary>
        ///     Gets or sets state that will be available to all nodes that are visited after this node.
        /// </summary>
        public virtual object NodeState { get; [param: CanBeNull] set; }

        /// <summary>
        ///     Creates a new node for the entity that is being traversed next in the graph.
        /// </summary>
        /// <param name="currentNode"> The node that the entity is being traversed from. </param>
        /// <param name="internalEntityEntry">
        ///      The internal entry tracking information about the entity being traversed to. 
        /// </param>
        /// <param name="reachedVia"> The navigation property that is being traversed to reach the new node. </param>
        /// <returns> The newly created node. </returns>
        public abstract TNode CreateNode(
            [NotNull] TNode currentNode, 
            [NotNull] InternalEntityEntry internalEntityEntry, 
            [NotNull] INavigation reachedVia);
    }
}
