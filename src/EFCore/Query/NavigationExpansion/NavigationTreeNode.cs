// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.NavigationExpansion
{
    public class NavigationTreeNode
    {
        private NavigationTreeNode(
            [NotNull] INavigation navigation,
            [NotNull] NavigationTreeNode parent,
            bool optional,
            bool include)
        {
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(parent, nameof(parent));

            Navigation = navigation;
            Parent = parent;
            Optional = optional;
            ToMapping = new List<string>();
            IsCollection = navigation.IsCollection();
            if (include)
            {
                ExpansionState = NavigationState.NotNeeded;
                IncludeState = NavigationState.Pending;
            }
            else
            {
                ExpansionState = NavigationState.Pending;
                IncludeState = NavigationState.NotNeeded;
            }

            // for ownership don't mark for include or expansion
            // just track the navigations in the tree in the original form
            // they will be expanded/translated later in the pipeline
            if (navigation.ForeignKey.IsOwnership)
            {
                if (include)
                {
                    IncludeState = NavigationState.Delayed;
                }
                else
                {
                    ExpansionState = NavigationState.Delayed;
                }

                ToMapping = parent.ToMapping.ToList();
                ToMapping.Add(navigation.Name);
            }

            foreach (var parentFromMapping in parent.FromMappings)
            {
                var newMapping = parentFromMapping.ToList();
                newMapping.Add(navigation.Name);
                FromMappings.Add(newMapping);
            }
        }

        private NavigationTreeNode(
            List<string> fromMapping,
            bool optional)
        {
            Optional = optional;
            FromMappings.Add(fromMapping.ToList());
            ToMapping = fromMapping.ToList();
            ExpansionState = NavigationState.Complete;
            IncludeState = NavigationState.NotNeeded;
        }

        public INavigation Navigation { get; private set; }
        public bool IsCollection { get; private set; }
        public bool Optional { get; private set; }
        public NavigationTreeNode Parent { get; private set; }
        public List<NavigationTreeNode> Children { get; private set; } = new List<NavigationTreeNode>();
        public NavigationState ExpansionState { get; set; }
        public NavigationState IncludeState { get; set; }
        public List<List<string>> FromMappings { get; set; } = new List<List<string>>();
        public List<string> ToMapping { get; set; }

        public static NavigationTreeNode CreateRoot(
            [NotNull] SourceMapping sourceMapping,
            [NotNull] List<string> fromMapping,
            bool optional)
        {
            Check.NotNull(sourceMapping, nameof(sourceMapping));
            Check.NotNull(fromMapping, nameof(fromMapping));

            return sourceMapping.NavigationTree ?? new NavigationTreeNode(fromMapping, optional);
        }

        public static NavigationTreeNode Create(
            [NotNull] SourceMapping sourceMapping,
            [NotNull] INavigation navigation,
            [NotNull] NavigationTreeNode parent,
            bool include)
        {
            Check.NotNull(sourceMapping, nameof(sourceMapping));
            Check.NotNull(navigation, nameof(navigation));
            Check.NotNull(parent, nameof(parent));

            var existingChild = parent.Children.SingleOrDefault(c => c.Navigation == navigation);
            if (existingChild != null)
            {
                if (navigation.ForeignKey.IsOwnership)
                {
                    if (include && existingChild.IncludeState == NavigationState.NotNeeded)
                    {
                        existingChild.IncludeState = NavigationState.Delayed;
                    }
                    else if (!include && existingChild.ExpansionState == NavigationState.NotNeeded)
                    {
                        existingChild.ExpansionState = NavigationState.Delayed;
                    }
                }
                else
                {
                    if (include && existingChild.IncludeState == NavigationState.NotNeeded)
                    {
                        existingChild.IncludeState = NavigationState.Pending;
                    }
                    else if (!include && existingChild.ExpansionState == NavigationState.NotNeeded)
                    {
                        existingChild.ExpansionState = NavigationState.Pending;
                    }
                }
                return existingChild;
            }

            // if (any) parent is optional, all children must be optional also
            // TODO: what about query filters?
            var optional = parent.Optional || !navigation.ForeignKey.IsRequired || !navigation.IsDependentToPrincipal();
            var result = new NavigationTreeNode(navigation, parent, optional, include);
            parent.Children.Add(result);

            return result;
        }

        public IEnumerable<NavigationTreeNode> Flatten()
        {
            yield return this;

            foreach (var child in Children.SelectMany(c => c.Flatten()))
            {
                yield return child;
            }
        }

        // TODO: just make property settable?
        public void MakeOptional()
        {
            Optional = true;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("'");
            builder.Append(Navigation?.Name ?? "");
            builder.Append("' Expand: '");
            builder.Append(ExpansionState);
            builder.Append("' Include: '");
            builder.Append(IncludeState);
            builder.Append("' Children: ");
            builder.Append(Children.Count);

            return builder.ToString();
        }
    }
}
