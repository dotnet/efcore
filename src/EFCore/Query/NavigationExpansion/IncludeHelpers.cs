// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public static class IncludeHelpers
    {
        public static void CopyIncludeInformation(NavigationTreeNode originalNavigationTree, NavigationTreeNode newNavigationTree, SourceMapping newSourceMapping)
        {
            foreach (var child in originalNavigationTree.Children.Where(n => n.Included == NavigationTreeNodeIncludeMode.ReferencePending || n.Included == NavigationTreeNodeIncludeMode.Collection))
            {
                var copy = NavigationTreeNode.Create(newSourceMapping, child.Navigation, newNavigationTree, true);
                CopyIncludeInformation(child, copy, newSourceMapping);
            }
        }

        public static TEntity IncludeMethod<TEntity>(TEntity entity, object includedNavigation, INavigation navigation)
        {
            if (entity == null)
            {
                return entity;
            }

            if (navigation.IsCollection() && includedNavigation == null)
            {
                return entity;
            }

            if (entity.GetType() == navigation.DeclaringEntityType.ClrType
                || entity.GetType().GetBaseTypes().Where(t => t == navigation.DeclaringEntityType.ClrType).Count() > 0)
            {
                if (navigation.PropertyInfo?.SetMethod != null)
                {
                    navigation.PropertyInfo.SetValue(entity, includedNavigation);
                }
                else
                {
                    navigation.FieldInfo.SetValue(entity, includedNavigation);
                }
            }

            return entity;
        }
    }
}
