// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
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
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
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
            [NotNull] IEntityEntryGraphIterator graphIterator) => _graphIterator = graphIterator;

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
            => _graphIterator.TraverseGraph(
                new EntityEntryGraphNode<(EntityState TargetState, EntityState StoreGenTargetState, bool Force)>(
                    rootEntry,
                    (targetState, storeGeneratedWithKeySetTargetState, forceStateWhenUnknownKey),
                    null,
                    null),
                PaintAction);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
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
                cancellationToken: cancellationToken);

            return true;
        }
    }
}
