// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;


namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     <para>
    ///         Provides access to change tracking information and operations for a node in a
    ///         graph of entities that is being traversed.
    ///     </para>
    ///     <para>
    ///         See <see cref="M:ChangeTracker.TrackGraph" /> for information on how graph nodes are used.
    ///     </para>
    /// </summary>
    /// <seealso href="https://aka.ms/efcore-docs-track-graph">Documentation for tracking entities in EF Core.</seealso>
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
            Check.NotNull(entry, nameof(entry));

            _entry = entry;
            _sourceEntry = sourceEntry;
            InboundNavigation = inboundNavigation;
        }

        /// <summary>
        ///     <para>
        ///         An <see cref="EntityEntry" /> for the entity instance from which a navigation property was traversed to the instance
        ///         represented by this node.
        ///     </para>
        ///     <para>
        ///         See <see cref="M:ChangeTracker.TrackGraph" /> for information on how graph nodes are used.
        ///     </para>
        /// </summary>
        public virtual EntityEntry? SourceEntry
            => _sourceEntry == null ? null : new EntityEntry(_sourceEntry);

        /// <summary>
        ///     <para>
        ///         Gets the navigation property that is being traversed to reach this node in the graph.
        ///     </para>
        ///     <para>
        ///         See <see cref="M:ChangeTracker.TrackGraph" /> for information on how graph nodes are used.
        ///     </para>
        /// </summary>
        public virtual INavigationBase? InboundNavigation { get; }

        /// <summary>
        ///     <para>
        ///         An <see cref="EntityEntry" /> for the entity instance represented by this node.
        ///     </para>
        ///     <para>
        ///         See <see cref="M:ChangeTracker.TrackGraph" /> for information on how graph nodes are used.
        ///     </para>
        /// </summary>
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
        [EntityFrameworkInternal]
        InternalEntityEntry IInfrastructure<InternalEntityEntry>.Instance
            => _entry;

        /// <summary>
        ///     Creates a new node for the entity that is being traversed next in the graph.
        /// </summary>
        /// <param name="currentNode"> The node that the entity is being traversed from. </param>
        /// <param name="internalEntityEntry">
        ///     The internal entry tracking information about the entity being traversed to.
        /// </param>
        /// <param name="reachedVia"> The navigation property that is being traversed to reach the new node. </param>
        /// <returns> The newly created node. </returns>
        public virtual EntityEntryGraphNode CreateNode(
            EntityEntryGraphNode currentNode,
            InternalEntityEntry internalEntityEntry,
            INavigationBase reachedVia)
        {
            Check.NotNull(currentNode, nameof(currentNode));
            Check.NotNull(internalEntityEntry, nameof(internalEntityEntry));
            Check.NotNull(reachedVia, nameof(reachedVia));

            return new EntityEntryGraphNode(
                internalEntityEntry,
                currentNode.Entry.GetInfrastructure(),
                reachedVia);
        }
    }
}
