// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
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
                new EntityEntryGraphNode(rootEntry, null)
                {
                    NodeState = entityState
                },
                PaintAction);

        private static bool PaintAction(EntityEntryGraphNode node)
        {
            var internalEntityEntry = node.GetInfrastructure();
            if ((internalEntityEntry.EntityState != EntityState.Detached)
                || ((node.InboundNavigation != null) && node.InboundNavigation.IsDependentToPrincipal()))
            {
                return false;
            }

            if (node.InboundNavigation != null
                && !internalEntityEntry.IsKeySet)
            {
                node.NodeState = EntityState.Added;
            }

            internalEntityEntry.SetEntityState((EntityState)node.NodeState, acceptChanges: true);

            return true;
        }
    }
}
