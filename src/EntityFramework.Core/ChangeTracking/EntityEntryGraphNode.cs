// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ChangeTracking
{
    public class EntityEntryGraphNode : EntityGraphNodeBase<EntityEntryGraphNode>
    {
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

        public virtual DbContext Context { get; }
        public new virtual EntityEntry Entry { get; }

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
