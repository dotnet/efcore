// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class EntityGraphAttacher : IEntityGraphAttacher
    {
        private readonly IEntityEntryGraphIterator _graphIterator;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityGraphAttacher(
            IEntityEntryGraphIterator graphIterator)
            => _graphIterator = graphIterator;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AttachGraph(
            InternalEntityEntry rootEntry,
            EntityState targetState,
            EntityState storeGeneratedWithKeySetTargetState,
            bool forceStateWhenUnknownKey)
        {
            try
            {
                rootEntry.StateManager.BeginAttachGraph();

                _graphIterator.TraverseGraph(
                    new EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)>(
                        rootEntry,
                        (targetState, storeGeneratedWithKeySetTargetState, forceStateWhenUnknownKey),
                        null,
                        null),
                    PaintAction);

                rootEntry.StateManager.CompleteAttachGraph();
            }
            catch
            {
                rootEntry.StateManager.AbortAttachGraph();
                throw;
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual async Task AttachGraphAsync(
            InternalEntityEntry rootEntry,
            EntityState targetState,
            EntityState storeGeneratedWithKeySetTargetState,
            bool forceStateWhenUnknownKey,
            CancellationToken cancellationToken = default)
        {
            try
            {
                rootEntry.StateManager.BeginAttachGraph();

                await _graphIterator.TraverseGraphAsync(
                    new EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)>(
                        rootEntry,
                        (targetState, storeGeneratedWithKeySetTargetState, forceStateWhenUnknownKey),
                        null,
                        null),
                    PaintActionAsync,
                    cancellationToken);

                rootEntry.StateManager.CompleteAttachGraph();
            }
            catch
            {
                rootEntry.StateManager.AbortAttachGraph();
                throw;
            }
        }

        private static bool PaintAction(
            EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)> node)
        {
            SetReferenceLoaded(node);

            var internalEntityEntry = node.GetInfrastructure();
            if (internalEntityEntry.EntityState != EntityState.Detached)
            {
                return false;
            }

            var (targetState, storeGenTargetState, force) = node.NodeState;

            var (isGenerated, isSet) = internalEntityEntry.IsKeySet;

            internalEntityEntry.SetEntityState(
                isSet
                    ? (isGenerated ? storeGenTargetState : targetState)
                    : EntityState.Added, // Key can only be not-set if it is store-generated
                acceptChanges: true,
                forceStateWhenUnknownKey: force ? (EntityState?)targetState : null);

            return true;
        }

        private static async Task<bool> PaintActionAsync(
            EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)> node,
            CancellationToken cancellationToken)
        {
            SetReferenceLoaded(node);

            var internalEntityEntry = node.GetInfrastructure();
            if (internalEntityEntry.EntityState != EntityState.Detached)
            {
                return false;
            }

            var (targetState, storeGenTargetState, force) = node.NodeState;

            var (isGenerated, isSet) = internalEntityEntry.IsKeySet;

            await internalEntityEntry.SetEntityStateAsync(
                    isSet
                        ? (isGenerated ? storeGenTargetState : targetState)
                        : EntityState.Added, // Key can only be not-set if it is store-generated
                    acceptChanges: true,
                    forceStateWhenUnknownKey: force ? (EntityState?)targetState : null,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return true;
        }

        private static void SetReferenceLoaded(
            EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)> node)
        {
            var inboundNavigation = node.InboundNavigation;
            if (inboundNavigation != null
                && !inboundNavigation.IsCollection)
            {
                node.SourceEntry!.GetInfrastructure().SetIsLoaded(inboundNavigation);
            }
        }
    }
}
