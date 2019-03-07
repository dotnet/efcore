// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     <para>
    ///         This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///         directly from your code. This API may change or be removed in future releases.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public class EntityEntryGraphIterator : IEntityEntryGraphIterator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void TraverseGraph<TState>(
            EntityEntryGraphNode<TState> node,
            Func<EntityEntryGraphNode<TState>, bool> handleNode)
        {
            if (!handleNode(node))
            {
                return;
            }

            var internalEntityEntry = node.GetInfrastructure();
            var navigations = ((EntityType)internalEntityEntry.EntityType).GetNavigations();
            var stateManager = internalEntityEntry.StateManager;

            foreach (var navigation in navigations)
            {
                var navigationValue = internalEntityEntry[navigation];

                if (navigationValue != null)
                {
                    var targetEntityType = navigation.GetTargetType();
                    if (navigation.IsCollection())
                    {
                        foreach (var relatedEntity in ((IEnumerable)navigationValue).Cast<object>().ToList())
                        {
                            var targetEntry = stateManager.GetOrCreateEntry(relatedEntity, targetEntityType);
                            TraverseGraph(
                                (EntityEntryGraphNode<TState>)node.CreateNode(node, targetEntry, navigation),
                                handleNode);
                        }
                    }
                    else
                    {
                        var targetEntry = stateManager.GetOrCreateEntry(navigationValue, targetEntityType);
                        TraverseGraph(
                            (EntityEntryGraphNode<TState>)node.CreateNode(node, targetEntry, navigation),
                            handleNode);
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual async Task TraverseGraphAsync<TState>(
            EntityEntryGraphNode<TState> node,
            Func<EntityEntryGraphNode<TState>, CancellationToken, Task<bool>> handleNode,
            CancellationToken cancellationToken = default)
        {
            if (!await handleNode(node, cancellationToken))
            {
                return;
            }

            var internalEntityEntry = node.GetInfrastructure();
            var navigations = internalEntityEntry.EntityType.GetNavigations();
            var stateManager = internalEntityEntry.StateManager;

            foreach (var navigation in navigations)
            {
                var navigationValue = internalEntityEntry[navigation];

                if (navigationValue != null)
                {
                    var targetType = navigation.GetTargetType();
                    if (navigation.IsCollection())
                    {
                        foreach (var relatedEntity in ((IEnumerable)navigationValue).Cast<object>().ToList())
                        {
                            var targetEntry = stateManager.GetOrCreateEntry(relatedEntity, targetType);
                            await TraverseGraphAsync(
                                (EntityEntryGraphNode<TState>)node.CreateNode(node, targetEntry, navigation),
                                handleNode,
                                cancellationToken);
                        }
                    }
                    else
                    {
                        var targetEntry = stateManager.GetOrCreateEntry(navigationValue, targetType);
                        await TraverseGraphAsync(
                            (EntityEntryGraphNode<TState>)node.CreateNode(node, targetEntry, navigation),
                            handleNode,
                            cancellationToken);
                    }
                }
            }
        }
    }
}
