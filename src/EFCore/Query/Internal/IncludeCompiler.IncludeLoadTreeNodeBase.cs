// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public partial class IncludeCompiler
    {
        private abstract class IncludeLoadTreeNodeBase
        {
            protected static void AddLoadPath(
                IncludeLoadTreeNodeBase node,
                IReadOnlyList<INavigation> navigationPath,
                int index)
            {
                while (index < navigationPath.Count)
                {
                    var navigation = navigationPath[index];
                    var childNode = node.Children.SingleOrDefault(n => n.Navigation == navigation);

                    if (childNode == null)
                    {
                        node.Children.Add(childNode = new IncludeLoadTreeNode(navigation));
                    }

                    node = childNode;
                    index = index + 1;
                }
            }

            protected ICollection<IncludeLoadTreeNode> Children { get; } = new List<IncludeLoadTreeNode>();
        }
    }
}
