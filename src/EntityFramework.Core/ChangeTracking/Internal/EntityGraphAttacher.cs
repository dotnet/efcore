// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

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
                new EntityEntryGraphNode(rootEntry, null)
                {
                    NodeState = entityState
                },
                PaintAction);

        private static bool PaintAction(EntityEntryGraphNode n)
        {
            var internalEntityEntry = n.GetInfrastructure();
            if ((internalEntityEntry.EntityState != EntityState.Detached)
                || ((n.InboundNavigation != null) && n.InboundNavigation.IsDependentToPrincipal()))
            {
                return false;
            }

            if (!internalEntityEntry.IsKeySet)
            {
                n.NodeState = EntityState.Added;
            }

            internalEntityEntry.SetEntityState((EntityState)n.NodeState, acceptChanges: true);

            return true;
        }
    }
}
