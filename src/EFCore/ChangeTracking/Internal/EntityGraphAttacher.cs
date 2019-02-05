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
        public EntityGraphAttacher(
            [NotNull] IEntityEntryGraphIterator graphIterator) => _graphIterator = graphIterator;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void AttachGraph(
            InternalEntityEntry rootEntry,
            EntityState targetState,
            EntityState storeGeneratedWithKeySetTargetState,
            bool forceStateWhenUnknownKey)
            => _graphIterator.TraverseGraph(
                new EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)>(
                    rootEntry,
                    (targetState, storeGeneratedWithKeySetTargetState, forceStateWhenUnknownKey),
                    null,
                    null),
                PaintAction);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task AttachGraphAsync(
            InternalEntityEntry rootEntry,
            EntityState targetState,
            EntityState storeGeneratedWithKeySetTargetState,
            bool forceStateWhenUnknownKey,
            CancellationToken cancellationToken = default)
            => _graphIterator.TraverseGraphAsync(
                new EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)>(
                    rootEntry,
                    (targetState, storeGeneratedWithKeySetTargetState, forceStateWhenUnknownKey),
                    null,
                    null),
                PaintActionAsync,
                cancellationToken);

        private static bool PaintAction(
            EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)> node)
        {
            var internalEntityEntry = node.GetInfrastructure();
            if (internalEntityEntry.EntityState != EntityState.Detached)
            {
                return false;
            }

            var nodeState = node.NodeState;

            var keyValueState = internalEntityEntry.IsKeySet;

            internalEntityEntry.SetEntityState(
                keyValueState.IsSet
                    ? (keyValueState.IsGenerated ? nodeState.StoreGenTargetState : nodeState.TargetState)
                    : EntityState.Added, // Key can only be not-set if it is store-generated
                acceptChanges: true,
                forceStateWhenUnknownKey: nodeState.Force ? (EntityState?)nodeState.TargetState : null);

            return true;
        }

        private static async Task<bool> PaintActionAsync(
            EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)> node,
            CancellationToken cancellationToken)
        {
            var internalEntityEntry = node.GetInfrastructure();
            if (internalEntityEntry.EntityState != EntityState.Detached)
            {
                return false;
            }

            var nodeState = node.NodeState;

            var keyValueState = internalEntityEntry.IsKeySet;

            await internalEntityEntry.SetEntityStateAsync(
                keyValueState.IsSet
                    ? (keyValueState.IsGenerated ? nodeState.StoreGenTargetState : nodeState.TargetState)
                    : EntityState.Added, // Key can only be not-set if it is store-generated
                acceptChanges: true,
                forceStateWhenUnknownKey: nodeState.Force ? (EntityState?)nodeState.TargetState : null,
                cancellationToken: cancellationToken);

            return true;
        }
    }
}
