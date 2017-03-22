// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;

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
        public virtual void TraverseGraph(EntityEntryGraphNode node, Func<EntityEntryGraphNode, bool> handleNode)
        {
            if (!handleNode(node))
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
                            TraverseGraph(
                                node.CreateNode(node, stateManager.GetOrCreateEntry(relatedEntity), navigation),
                                handleNode);
                        }
                    }
                    else
                    {
                        var targetEntityType = navigation.GetTargetType();
                        var targetEntry = targetEntityType.HasDelegatedIdentity()
                            ? stateManager.GetOrCreateEntry(navigationValue, targetEntityType)
                            : stateManager.GetOrCreateEntry(navigationValue);
                        TraverseGraph(node.CreateNode(node, targetEntry, navigation), handleNode);
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual async Task TraverseGraphAsync(
            EntityEntryGraphNode node,
            Func<EntityEntryGraphNode, CancellationToken, Task<bool>> handleNode,
            CancellationToken cancellationToken = default(CancellationToken))
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
                    if (navigation.IsCollection())
                    {
                        foreach (var relatedEntity in ((IEnumerable)navigationValue).Cast<object>().ToList())
                        {
                            await TraverseGraphAsync(
                                node.CreateNode(node, stateManager.GetOrCreateEntry(relatedEntity), navigation),
                                handleNode,
                                cancellationToken);
                        }
                    }
                    else
                    {
                        var targetType = navigation.GetTargetType();
                        var entry = targetType.HasDelegatedIdentity()
                            ? stateManager.GetOrCreateEntry(navigationValue, targetType)
                            : stateManager.GetOrCreateEntry(navigationValue);
                        await TraverseGraphAsync(
                            node.CreateNode(node, entry, navigation),
                            handleNode,
                            cancellationToken);
                    }
                }
            }
        }
    }
}
