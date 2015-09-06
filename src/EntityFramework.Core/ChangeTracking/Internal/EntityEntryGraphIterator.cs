// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class EntityEntryGraphIterator : IEntityEntryGraphIterator
    {
        public virtual void TraverseGraph<TNode>(TNode node, Func<TNode, bool> handleNode)
            where TNode : EntityGraphNodeBase<TNode>
        {
            if (!handleNode(node))
            {
                return;
            }

            var navigations = node.Entry.EntityType.GetNavigations();
            var stateManager = node.Entry.StateManager;

            foreach (var navigation in navigations)
            {
                var navigationValue = node.Entry[navigation];

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
