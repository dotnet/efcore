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
                    if (navigation.IsCollection())
                    {
                        foreach (var relatedEntity in ((IEnumerable)navigationValue).Cast<object>().ToList())
                        {
                            TraverseGraph(
                                node.CreateNode(node, stateManager.GetOrCreateEntry(relatedEntity), navigation),
                                state,
                                handleNode);
                        }
                    }
                    else
                    {
                        var targetEntityType = navigation.GetTargetType();
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
                    if (navigation.IsCollection())
                    {
                        foreach (var relatedEntity in ((IEnumerable)navigationValue).Cast<object>().ToList())
                        {
                            await TraverseGraphAsync(
                                node.CreateNode(node, stateManager.GetOrCreateEntry(relatedEntity), navigation),
                                state,
                                handleNode,
                                cancellationToken);
                        }
                    }
                    else
                    {
                        var targetType = navigation.GetTargetType();
                        var entry = targetType.HasDefiningNavigation()
                            ? stateManager.GetOrCreateEntry(navigationValue, targetType)
                            : stateManager.GetOrCreateEntry(navigationValue);
                        await TraverseGraphAsync(
                            node.CreateNode(node, entry, navigation),
                            state,
                            handleNode,
                            cancellationToken);
                    }
                }
            }
        }
    }
}
