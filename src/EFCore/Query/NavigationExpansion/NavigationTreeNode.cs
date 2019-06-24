// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
            if (include)
            {
                ExpansionMode = NavigationTreeNodeExpansionMode.NotNeeded;
                Included = navigation.IsCollection()
                    ? NavigationTreeNodeIncludeMode.Collection
                    : NavigationTreeNodeIncludeMode.ReferencePending;
            }
            else
            {
                ExpansionMode = navigation.IsCollection()
                    ? NavigationTreeNodeExpansionMode.Collection
                    : NavigationTreeNodeExpansionMode.ReferencePending;
                Included = NavigationTreeNodeIncludeMode.NotNeeded;
            }

            // for ownership don't mark for include or expansion
            // just track the navigations in the tree in the original form
            // they will be expanded/translated later in the pipeline
            if (navigation.ForeignKey.IsOwnership)
            {
                ExpansionMode = NavigationTreeNodeExpansionMode.NotNeeded;
                Included = NavigationTreeNodeIncludeMode.NotNeeded;

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
            ExpansionMode = NavigationTreeNodeExpansionMode.ReferenceComplete;
            Included = NavigationTreeNodeIncludeMode.NotNeeded;
        }

        public INavigation Navigation { get; private set; }
        public bool Optional { get; private set; }
        public NavigationTreeNode Parent { get; private set; }
        public List<NavigationTreeNode> Children { get; private set; } = new List<NavigationTreeNode>();
        public NavigationTreeNodeExpansionMode ExpansionMode { get; set; }
        public NavigationTreeNodeIncludeMode Included { get; set; }

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

            var existingChild = parent.Children.Where(c => c.Navigation == navigation).SingleOrDefault();
            if (existingChild != null)
            {
                if (!navigation.ForeignKey.IsOwnership)
                {
                    if (include && existingChild.Included == NavigationTreeNodeIncludeMode.NotNeeded)
                    {
                        existingChild.Included = navigation.IsCollection()
                            ? NavigationTreeNodeIncludeMode.Collection
                            : NavigationTreeNodeIncludeMode.ReferencePending;
                    }
                    else if (!include && existingChild.ExpansionMode == NavigationTreeNodeExpansionMode.NotNeeded)
                    {
                        existingChild.ExpansionMode = navigation.IsCollection()
                            ? NavigationTreeNodeExpansionMode.Collection
                            : NavigationTreeNodeExpansionMode.ReferencePending;
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

        private static void PrependFromMappings(NavigationTreeNode navigationTreeNode, List<List<string>> fromMappingsToPrepend)
        {
            var newFromMappings = new List<List<string>>();
            foreach (var parentFromMapping in fromMappingsToPrepend)
            {
                foreach (var fromMapping in navigationTreeNode.FromMappings)
                {
                    var newMapping = parentFromMapping.ToList();
                    newMapping.AddRange(fromMapping);
                    newFromMappings.Add(newMapping);
                }
            }

            navigationTreeNode.FromMappings = newFromMappings;
            foreach (var child in navigationTreeNode.Children)
            {
                PrependFromMappings(child, fromMappingsToPrepend);
            }
        }

        public void AddChild([NotNull] NavigationTreeNode childNode, bool propagateFromMappings = true)
        {
            Check.NotNull(childNode, nameof(childNode));

            // when adding the first child - propagate FromMappings from the parent
            if (propagateFromMappings)
            {
                PrependFromMappings(childNode, FromMappings);
            }

            var existingChild = Children.Where(c => c.Navigation == childNode.Navigation).SingleOrDefault();
            if (existingChild != null)
            {
                // if the child exisits, copy ToMappings, add new unique FromMappings and try adding it's children
                // however for those children we don't need to re-propagate the mappings, since they are already in place
                var newMappings = childNode.FromMappings.Where(m => !existingChild.FromMappings.Any(em => em.SequenceEqual(m)));
                existingChild.ToMapping = childNode.ToMapping;
                existingChild.FromMappings.AddRange(newMappings);
                foreach (var grandChild in existingChild.Children)
                {
                    existingChild.AddChild(grandChild, propagateFromMappings: false);
                }
            }
            else
            {
                Children.Add(childNode);
                childNode.Parent = this;
            }
        }

        public List<NavigationTreeNode> Flatten()
        {
            var result = new List<NavigationTreeNode>();
            result.Add(this);

            foreach (var child in Children)
            {
                result.AddRange(child.Flatten());
            }

            return result;
        }

        // TODO: just make property settable?
        public void MakeOptional()
        {
            Optional = true;
        }

        // TODO: hack - refactor this so that it's not needed
        internal void SetNavigation(INavigation navigation)
        {
            Navigation = navigation;
            PrependFromMappings(this, new List<List<string>> { new List<string> { navigation.Name } });
        }
    }
}
