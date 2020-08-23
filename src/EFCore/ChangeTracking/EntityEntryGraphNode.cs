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
    public class EntityEntryGraphNode : IInfrastructure<InternalEntityEntry>
    {
        private readonly InternalEntityEntry _entry;
        private readonly InternalEntityEntry _sourceEntry;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [DebuggerStepThrough]
        [EntityFrameworkInternal]
        public EntityEntryGraphNode(
            [NotNull] InternalEntityEntry entry,
            [CanBeNull] InternalEntityEntry sourceEntry,
            [CanBeNull] INavigationBase inboundNavigation)
        {
            Check.NotNull(entry, nameof(entry));

            _entry = entry;
            _sourceEntry = sourceEntry;
            InboundNavigation = inboundNavigation;
        }

        /// <summary>
        ///     Gets the entry tracking information about this entity.
        /// </summary>
        public virtual EntityEntry SourceEntry
            => _sourceEntry == null ? null : new EntityEntry(_sourceEntry);

        /// <summary>
        ///     Gets the navigation property that is being traversed to reach this node in the graph.
        /// </summary>
        public virtual INavigationBase InboundNavigation { get; }

        /// <summary>
        ///     Gets the entry tracking information about this entity.
        /// </summary>
        public virtual EntityEntry Entry
            => new EntityEntry(_entry);

        /// <summary>
        ///     <para>
        ///         Gets the internal entry that is tracking information about this entity.
        ///     </para>
        ///     <para>
        ///         This property is intended for use by extension methods. It is not intended to be used in
        ///         application code.
        ///     </para>
        /// </summary>
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
            [NotNull] EntityEntryGraphNode currentNode,
            [NotNull] InternalEntityEntry internalEntityEntry,
            [NotNull] INavigationBase reachedVia)
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
