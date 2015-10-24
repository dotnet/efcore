// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class EntityEntryGraphIterator : IEntityEntryGraphIterator
    {
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
                        foreach (var relatedEntity in (IEnumerable)navigationValue)
                        {
                            TraverseGraph(
                                node.CreateNode(node, stateManager.GetOrCreateEntry(relatedEntity), navigation),
                                handleNode);
                        }
                    }
                    else
                    {
                        TraverseGraph(
                            node.CreateNode(node, stateManager.GetOrCreateEntry(navigationValue), navigation),
                            handleNode);
                    }
                }
            }
        }
    }
}
