// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class EntityEntryGraphIterator : IEntityEntryGraphIterator
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void TraverseGraph<TState>(
            EntityEntryGraphNode node,
            TState state,
            Func<EntityEntryGraphNode, TState, bool> handleNode)
        {
            if (!handleNode(node, state))
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
                            var targetEntry = targetEntityType.HasDefiningNavigation()
                                ? stateManager.GetOrCreateEntry(relatedEntity, targetEntityType)
                                : stateManager.GetOrCreateEntry(relatedEntity);
                            TraverseGraph(
                                node.CreateNode(node, targetEntry, navigation),
                                state,
                                handleNode);
                        }
                    }
                    else
                    {
                        var targetEntry = targetEntityType.HasDefiningNavigation()
                            ? stateManager.GetOrCreateEntry(navigationValue, targetEntityType)
                            : stateManager.GetOrCreateEntry(navigationValue);
                        TraverseGraph(
                            node.CreateNode(node, targetEntry, navigation),
                            state,
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
            EntityEntryGraphNode node,
            TState state,
            Func<EntityEntryGraphNode, TState, CancellationToken, Task<bool>> handleNode,
            CancellationToken cancellationToken = default)
        {
            if (!await handleNode(node, state, cancellationToken))
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
                            var targetEntry = targetType.HasDefiningNavigation()
                                ? stateManager.GetOrCreateEntry(relatedEntity, targetType)
                                : stateManager.GetOrCreateEntry(relatedEntity);
                            await TraverseGraphAsync(
                                node.CreateNode(node, targetEntry, navigation),
                                state,
                                handleNode,
                                cancellationToken);
                        }
                    }
                    else
                    {
                        var targetEntry = targetType.HasDefiningNavigation()
                            ? stateManager.GetOrCreateEntry(navigationValue, targetType)
                            : stateManager.GetOrCreateEntry(navigationValue);
                        await TraverseGraphAsync(
                            node.CreateNode(node, targetEntry, navigation),
                            state,
                            handleNode,
                            cancellationToken);
                    }
                }
            }
        }
    }
}
