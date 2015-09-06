// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class EntityGraphAttacher : IEntityGraphAttacher
    {
        private readonly IEntityEntryGraphIterator _graphIterator;

        public EntityGraphAttacher([NotNull] IEntityEntryGraphIterator graphIterator)
        {
            _graphIterator = graphIterator;
        }

        public virtual void AttachGraph(InternalEntityEntry rootEntry, EntityState entityState)
            => _graphIterator.TraverseGraph(
                new InternalEntityEntryGraphNode(rootEntry, null)
                    {
                        NodeState = entityState
                    },
                PaintAction);

        private static bool PaintAction(InternalEntityEntryGraphNode n)
        {
            if (n.Entry.EntityState != EntityState.Detached
                || (n.InboundNavigation != null && n.InboundNavigation.PointsToPrincipal()))
            {
                return false;
            }

            if (!n.Entry.IsKeySet)
            {
                n.NodeState = EntityState.Added;
            }

            n.Entry.SetEntityState((EntityState)n.NodeState, acceptChanges: true);

            return true;
        }

        private class InternalEntityEntryGraphNode : EntityGraphNodeBase<InternalEntityEntryGraphNode>
        {
            public InternalEntityEntryGraphNode(InternalEntityEntry internalEntityEntry, INavigation reachedVia)
                : base(internalEntityEntry, reachedVia)
            {
            }

            public override InternalEntityEntryGraphNode CreateNode(
                InternalEntityEntryGraphNode currentNode, InternalEntityEntry internalEntityEntry, INavigation reachedVia)
                => new InternalEntityEntryGraphNode(internalEntityEntry, reachedVia) { NodeState = currentNode.NodeState };
        }
    }
}
