// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityGraphAttacher : IEntityGraphAttacher
    {
        private readonly IEntityEntryGraphIterator _graphIterator;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityGraphAttacher([NotNull] IEntityEntryGraphIterator graphIterator)
        {
            _graphIterator = graphIterator;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AttachGraph(InternalEntityEntry rootEntry, EntityState entityState)
            => _graphIterator.TraverseGraph(
                new EntityEntryGraphNode(rootEntry, null)
                {
                    NodeState = entityState
                },
                PaintAction);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task AttachGraphAsync(
            InternalEntityEntry rootEntry,
            EntityState entityState,
            CancellationToken cancellationToken = default(CancellationToken))
            => _graphIterator.TraverseGraphAsync(
                new EntityEntryGraphNode(rootEntry, null)
                {
                    NodeState = entityState
                },
                PaintActionAsync,
                cancellationToken);

        private static InternalEntityEntry PrepareToPaint(EntityEntryGraphNode node)
        {
            var internalEntityEntry = node.GetInfrastructure();
            if (internalEntityEntry.EntityState != EntityState.Detached)
            {
                return null;
            }

            if (node.InboundNavigation != null
                && !internalEntityEntry.IsKeySet)
            {
                node.NodeState = EntityState.Added;
            }

            return internalEntityEntry;
        }

        private static bool PaintAction(EntityEntryGraphNode node)
        {
            var internalEntityEntry = PrepareToPaint(node);
            if (internalEntityEntry == null)
            {
                return false;
            }

            internalEntityEntry.SetEntityState((EntityState)node.NodeState, acceptChanges: true);

            return true;
        }

        private static async Task<bool> PaintActionAsync(EntityEntryGraphNode node, CancellationToken cancellationToken)
        {
            var internalEntityEntry = PrepareToPaint(node);
            if (internalEntityEntry == null)
            {
                return false;
            }

            await internalEntityEntry.SetEntityStateAsync(
                (EntityState)node.NodeState, 
                acceptChanges: true,
                cancellationToken: cancellationToken);

            return true;
        }
    }
}
