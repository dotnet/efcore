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

        // Stored for perf
        private static readonly object[] _boxedEntityStates =
            { EntityState.Detached, EntityState.Unchanged, EntityState.Deleted, EntityState.Modified, EntityState.Added };

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public EntityGraphAttacher(
            [NotNull] IEntityEntryGraphIterator graphIterator) => _graphIterator = graphIterator;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AttachGraph(InternalEntityEntry rootEntry, EntityState entityState, bool forceStateWhenUnknownKey)
            => _graphIterator.TraverseGraph(
                new EntityEntryGraphNode(rootEntry, null, null)
                {
                    NodeState = _boxedEntityStates[(int)entityState]
                },
                forceStateWhenUnknownKey,
                PaintAction);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task AttachGraphAsync(
            InternalEntityEntry rootEntry,
            EntityState entityState,
            bool forceStateWhenUnknownKey,
            CancellationToken cancellationToken = default)
            => _graphIterator.TraverseGraphAsync(
                new EntityEntryGraphNode(rootEntry, null, null)
                {
                    NodeState = _boxedEntityStates[(int)entityState]
                },
                forceStateWhenUnknownKey,
                PaintActionAsync,
                cancellationToken);

        private static bool PaintAction(EntityEntryGraphNode node, bool force)
        {
            var internalEntityEntry = node.GetInfrastructure();
            if (internalEntityEntry.EntityState != EntityState.Detached)
            {
                return false;
            }

            var entityState = (EntityState)node.NodeState;

            internalEntityEntry.SetEntityState(
                internalEntityEntry.IsKeySet
                    ? entityState
                    : EntityState.Added,
                acceptChanges: true,
                forceStateWhenUnknownKey: force ? (EntityState?)entityState : null);

            return true;
        }

        private static async Task<bool> PaintActionAsync(EntityEntryGraphNode node, bool force, CancellationToken cancellationToken)
        {
            var internalEntityEntry = node.GetInfrastructure();
            if (internalEntityEntry.EntityState != EntityState.Detached)
            {
                return false;
            }

            var entityState = (EntityState)node.NodeState;

            await internalEntityEntry.SetEntityStateAsync(
                internalEntityEntry.IsKeySet
                    ? entityState
                    : EntityState.Added,
                acceptChanges: true,
                forceStateWhenUnknownKey: force ? (EntityState?)entityState : null,
                cancellationToken: cancellationToken);

            return true;
        }
    }
}
