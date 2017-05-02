// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public partial class ConventionDispatcher
    {
        private abstract class ConventionVisitor
        {
            public virtual ConventionNode Visit(ConventionNode node) => node?.Accept(this);

            public virtual ConventionScope VisitConventionScope(ConventionScope node)
            {
                List<ConventionNode> visitedNodes = null;
                foreach (var conventionNode in node.Children)
                {
                    var visitedNode = Visit(conventionNode);
                    if (visitedNode == null)
                    {
                        continue;
                    }
                    if (visitedNodes == null)
                    {
                        visitedNodes = new List<ConventionNode>();
                    }
                    visitedNodes.Add(visitedNode);
                }
                return (visitedNodes?.Count ?? 0) == 0 ? null : new ConventionScope(node.Parent, visitedNodes);
            }

            public virtual OnEntityTypeAddedNode VisitOnEntityTypeAdded(OnEntityTypeAddedNode node) => node;
            public virtual OnEntityTypeIgnoredNode VisitOnEntityTypeIgnored(OnEntityTypeIgnoredNode node) => node;
            public virtual OnEntityTypeMemberIgnoredNode VisitOnEntityTypeMemberIgnored(OnEntityTypeMemberIgnoredNode node) => node;
            public virtual OnBaseEntityTypeSetNode VisitOnBaseEntityTypeSet(OnBaseEntityTypeSetNode node) => node;
            public virtual OnEntityTypeAnnotationSetNode VisitOnEntityTypeAnnotationSet(OnEntityTypeAnnotationSetNode node) => node;
            public virtual OnForeignKeyAddedNode VisitOnForeignKeyAdded(OnForeignKeyAddedNode node) => node;
            public virtual OnForeignKeyRemovedNode VisitOnForeignKeyRemoved(OnForeignKeyRemovedNode node) => node;
            public virtual OnKeyAddedNode VisitOnKeyAdded(OnKeyAddedNode node) => node;
            public virtual OnKeyRemovedNode VisitOnKeyRemoved(OnKeyRemovedNode node) => node;
            public virtual OnPrimaryKeySetNode VisitOnPrimaryKeySet(OnPrimaryKeySetNode node) => node;
            public virtual OnIndexAddedNode VisitOnIndexAdded(OnIndexAddedNode node) => node;
            public virtual OnIndexRemovedNode VisitOnIndexRemoved(OnIndexRemovedNode node) => node;
            public virtual OnIndexUniquenessChangedNode VisitOnIndexUniquenessChanged(OnIndexUniquenessChangedNode node) => node;
            public virtual OnIndexAnnotationSetNode VisitOnIndexAnnotationSet(OnIndexAnnotationSetNode node) => node;
            public virtual OnNavigationAddedNode VisitOnNavigationAdded(OnNavigationAddedNode node) => node;
            public virtual OnNavigationRemovedNode VisitOnNavigationRemoved(OnNavigationRemovedNode node) => node;
            public virtual OnForeignKeyUniquenessChangedNode VisitOnForeignKeyUniquenessChanged(OnForeignKeyUniquenessChangedNode node) => node;
            public virtual OnForeignKeyOwnershipChangedNode VisitOnForeignKeyOwnershipChanged(OnForeignKeyOwnershipChangedNode node) => node;
            public virtual OnPrincipalEndSetNode VisitOnPrincipalEndSet(OnPrincipalEndSetNode node) => node;
            public virtual OnPropertyAddedNode VisitOnPropertyAdded(OnPropertyAddedNode node) => node;
            public virtual OnPropertyNullableChangedNode VisitOnPropertyNullableChanged(OnPropertyNullableChangedNode node) => node;
            public virtual OnPropertyFieldChangedNode VisitOnPropertyFieldChanged(OnPropertyFieldChangedNode node) => node;
            public virtual OnPropertyAnnotationSetNode VisitOnPropertyAnnotationSet(OnPropertyAnnotationSetNode node) => node;
        }

        private class RunVisitor : ConventionVisitor
        {
            public RunVisitor(ConventionDispatcher dispatcher)
            {
                Dispatcher = dispatcher;
            }

            public override OnEntityTypeAddedNode VisitOnEntityTypeAdded(OnEntityTypeAddedNode node)
            {
                Dispatcher._immediateConventionScope.OnEntityTypeAdded(node.EntityTypeBuilder);
                return null;
            }

            public override OnEntityTypeIgnoredNode VisitOnEntityTypeIgnored(OnEntityTypeIgnoredNode node)
            {
                Dispatcher._immediateConventionScope.OnEntityTypeIgnored(node.ModelBuilder, node.Name, node.Type);
                return null;
            }

            public override OnEntityTypeMemberIgnoredNode VisitOnEntityTypeMemberIgnored(OnEntityTypeMemberIgnoredNode node)
            {
                Dispatcher._immediateConventionScope.OnEntityTypeMemberIgnored(node.EntityTypeBuilder, node.IgnoredMemberName);
                return null;
            }

            public override OnBaseEntityTypeSetNode VisitOnBaseEntityTypeSet(OnBaseEntityTypeSetNode node)
            {
                Dispatcher._immediateConventionScope.OnBaseEntityTypeSet(node.EntityTypeBuilder, node.PreviousBaseType);
                return null;
            }

            public override OnEntityTypeAnnotationSetNode VisitOnEntityTypeAnnotationSet(OnEntityTypeAnnotationSetNode node)
            {
                Dispatcher._immediateConventionScope.OnEntityTypeAnnotationSet(node.EntityTypeBuilder, node.Name, node.Annotation, node.OldAnnotation);
                return null;
            }

            public override OnForeignKeyAddedNode VisitOnForeignKeyAdded(OnForeignKeyAddedNode node)
            {
                Dispatcher._immediateConventionScope.OnForeignKeyAdded(node.RelationshipBuilder);
                return null;
            }

            public override OnForeignKeyRemovedNode VisitOnForeignKeyRemoved(OnForeignKeyRemovedNode node)
            {
                Dispatcher._immediateConventionScope.OnForeignKeyRemoved(node.EntityTypeBuilder, node.ForeignKey);
                return null;
            }

            public override OnKeyAddedNode VisitOnKeyAdded(OnKeyAddedNode node)
            {
                Dispatcher._immediateConventionScope.OnKeyAdded(node.KeyBuilder);
                return null;
            }

            public override OnKeyRemovedNode VisitOnKeyRemoved(OnKeyRemovedNode node)
            {
                Dispatcher._immediateConventionScope.OnKeyRemoved(node.EntityTypeBuilder, node.Key);
                return null;
            }

            public override OnPrimaryKeySetNode VisitOnPrimaryKeySet(OnPrimaryKeySetNode node)
            {
                Dispatcher._immediateConventionScope.OnPrimaryKeySet(node.EntityTypeBuilder, node.PreviousPrimaryKey);
                return null;
            }

            public override OnIndexAddedNode VisitOnIndexAdded(OnIndexAddedNode node)
            {
                Dispatcher._immediateConventionScope.OnIndexAdded(node.IndexBuilder);
                return null;
            }

            public override OnIndexRemovedNode VisitOnIndexRemoved(OnIndexRemovedNode node)
            {
                Dispatcher._immediateConventionScope.OnIndexRemoved(node.EntityTypeBuilder, node.Index);
                return null;
            }

            public override OnIndexUniquenessChangedNode VisitOnIndexUniquenessChanged(OnIndexUniquenessChangedNode node)
            {
                Dispatcher._immediateConventionScope.OnIndexUniquenessChanged(node.IndexBuilder);
                return null;
            }

            public override OnIndexAnnotationSetNode VisitOnIndexAnnotationSet(OnIndexAnnotationSetNode node)
            {
                Dispatcher._immediateConventionScope.OnIndexAnnotationSet(node.IndexBuilder, node.Name, node.Annotation, node.OldAnnotation);
                return null;
            }

            public override OnNavigationAddedNode VisitOnNavigationAdded(OnNavigationAddedNode node)
            {
                Dispatcher._immediateConventionScope.OnNavigationAdded(node.RelationshipBuilder, node.Navigation);
                return null;
            }

            public override OnNavigationRemovedNode VisitOnNavigationRemoved(OnNavigationRemovedNode node)
            {
                Dispatcher._immediateConventionScope.OnNavigationRemoved(
                    node.SourceEntityTypeBuilder, node.TargetEntityTypeBuilder, node.NavigationName, node.PropertyInfo);
                return null;
            }

            public override OnForeignKeyUniquenessChangedNode VisitOnForeignKeyUniquenessChanged(OnForeignKeyUniquenessChangedNode node)
            {
                Dispatcher._immediateConventionScope.OnForeignKeyUniquenessChanged(node.RelationshipBuilder);
                return null;
            }

            public override OnForeignKeyOwnershipChangedNode VisitOnForeignKeyOwnershipChanged(OnForeignKeyOwnershipChangedNode node)
            {
                Dispatcher._immediateConventionScope.OnForeignKeyOwnershipChanged(node.RelationshipBuilder);
                return null;
            }

            public override OnPrincipalEndSetNode VisitOnPrincipalEndSet(OnPrincipalEndSetNode node)
            {
                Dispatcher._immediateConventionScope.OnPrincipalEndSet(node.RelationshipBuilder);
                return null;
            }

            public override OnPropertyAddedNode VisitOnPropertyAdded(OnPropertyAddedNode node)
            {
                Dispatcher._immediateConventionScope.OnPropertyAdded(node.PropertyBuilder);
                return null;
            }

            public override OnPropertyNullableChangedNode VisitOnPropertyNullableChanged(OnPropertyNullableChangedNode node)
            {
                Dispatcher._immediateConventionScope.OnPropertyNullableChanged(node.PropertyBuilder);
                return null;
            }

            public override OnPropertyFieldChangedNode VisitOnPropertyFieldChanged(OnPropertyFieldChangedNode node)
            {
                Dispatcher._immediateConventionScope.OnPropertyFieldChanged(node.PropertyBuilder, node.OldFieldInfo);
                return null;
            }

            public override OnPropertyAnnotationSetNode VisitOnPropertyAnnotationSet(OnPropertyAnnotationSetNode node)
            {
                Dispatcher._immediateConventionScope.OnPropertyAnnotationSet(node.PropertyBuilder, node.Name, node.Annotation, node.OldAnnotation);
                return null;
            }

            private ConventionDispatcher Dispatcher { get; }
        }
    }
}
