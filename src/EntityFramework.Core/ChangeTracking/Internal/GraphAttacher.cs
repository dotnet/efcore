// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class GraphAttacher : IGraphAttacher
    {
        private readonly IStateManager _stateManager;
        private readonly IEntityEntryGraphIterator _graphIterator;

        public GraphAttacher(
            [NotNull] IStateManager stateManager, 
            [NotNull] IEntityEntryGraphIterator graphIterator)
        {
            _stateManager = stateManager;
            _graphIterator = graphIterator;
        }

        public virtual void AttachGraph(object rootEntity, EntityState entityState)
            => _graphIterator.TraverseGraph(
                new InternalEntityEntryGraphNode(_stateManager.GetOrCreateEntry(rootEntity), null)
                    {
                        NodeState = entityState
                    },
                PaintAction);

        private static bool PaintAction(InternalEntityEntryGraphNode n)
        {
            if (n.Entry.EntityState != EntityState.Detached
                || (n.ReachedVia != null && n.ReachedVia.PointsToPrincipal()))
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

            public override InternalEntityEntryGraphNode NewNode(
                InternalEntityEntryGraphNode currentNode, InternalEntityEntry internalEntityEntry, INavigation reachedVia)
                => new InternalEntityEntryGraphNode(internalEntityEntry, reachedVia) { NodeState = currentNode.NodeState };
        }
    }
}
