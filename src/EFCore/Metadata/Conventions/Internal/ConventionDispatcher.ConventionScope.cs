// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public partial class ConventionDispatcher
    {
        private abstract class ConventionScope : ConventionNode
        {
            public virtual ConventionScope Parent => null;
            public virtual IReadOnlyList<ConventionNode> Children => null;

            public int GetLeafCount()
            {
                if (Children == null)
                {
                    return 0;
                }

                var scopesToVisit = new Queue<ConventionScope>();
                scopesToVisit.Enqueue(this);
                var leafCount = 0;
                while (scopesToVisit.Count > 0)
                {
                    var scope = scopesToVisit.Dequeue();
                    if (scope.Children == null)
                    {
                        continue;
                    }

                    foreach (var conventionNode in scope.Children)
                    {
                        if (conventionNode is ConventionScope nextScope)
                        {
                            scopesToVisit.Enqueue(nextScope);
                        }
                        else
                        {
                            leafCount++;
                        }
                    }
                }

                return leafCount;
            }

            public abstract IConventionEntityTypeBuilder OnEntityTypeAdded([NotNull] IConventionEntityTypeBuilder entityTypeBuilder);

            public abstract IConventionAnnotation OnEntityTypeAnnotationChanged(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation);

            public abstract IConventionEntityType OnEntityTypeBaseTypeChanged(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [CanBeNull] IConventionEntityType newBaseType,
                [CanBeNull] IConventionEntityType previousBaseType);

            public abstract string OnEntityTypeIgnored(
                [NotNull] IConventionModelBuilder modelBuilder, [NotNull] string name, [CanBeNull] Type type);

            public abstract string OnEntityTypeMemberIgnored([NotNull] IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] string name);

            public abstract IConventionKey OnEntityTypePrimaryKeyChanged(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [CanBeNull] IConventionKey newPrimaryKey,
                [CanBeNull] IConventionKey previousPrimaryKey);

            public abstract IConventionEntityType OnEntityTypeRemoved(
                [NotNull] IConventionModelBuilder modelBuilder, [NotNull] IConventionEntityType entityType);

            public abstract IConventionRelationshipBuilder OnForeignKeyAdded([NotNull] IConventionRelationshipBuilder relationshipBuilder);

            public abstract IConventionAnnotation OnForeignKeyAnnotationChanged(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull]IConventionAnnotation oldAnnotation);

            public abstract IConventionRelationshipBuilder OnForeignKeyOwnershipChanged(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder);

            public abstract IConventionRelationshipBuilder OnForeignKeyPrincipalEndChanged(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder);

            public abstract IConventionRelationshipBuilder OnForeignKeyPropertiesChanged(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder,
                [NotNull] IReadOnlyList<IConventionProperty> oldDependentProperties,
                [NotNull] IConventionKey oldPrincipalKey);

            public abstract IConventionForeignKey OnForeignKeyRemoved(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] IConventionForeignKey foreignKey);

            public abstract IConventionRelationshipBuilder OnForeignKeyRequirednessChanged(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder);

            public abstract IConventionRelationshipBuilder OnForeignKeyUniquenessChanged(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder);

            public abstract IConventionIndexBuilder OnIndexAdded([NotNull] IConventionIndexBuilder indexBuilder);

            public abstract IConventionAnnotation OnIndexAnnotationChanged(
                [NotNull] IConventionIndexBuilder indexBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation);

            public abstract IConventionIndex OnIndexRemoved(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] IConventionIndex index);

            public abstract IConventionIndexBuilder OnIndexUniquenessChanged([NotNull] IConventionIndexBuilder indexBuilder);
            public abstract IConventionKeyBuilder OnKeyAdded([NotNull] IConventionKeyBuilder keyBuilder);

            public abstract IConventionAnnotation OnKeyAnnotationChanged(
                [NotNull] IConventionKeyBuilder keyBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation);

            public abstract IConventionKey OnKeyRemoved([NotNull] IConventionEntityTypeBuilder entityTypeBuilder, [NotNull] IConventionKey key);

            public abstract IConventionAnnotation OnModelAnnotationChanged(
                [NotNull] IConventionModelBuilder modelBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation);

            public abstract IConventionNavigation OnNavigationAdded(
                [NotNull] IConventionRelationshipBuilder relationshipBuilder, [NotNull] IConventionNavigation navigation);

            public abstract string OnNavigationRemoved(
                [NotNull] IConventionEntityTypeBuilder sourceEntityTypeBuilder,
                [NotNull] IConventionEntityTypeBuilder targetEntityTypeBuilder,
                [NotNull] string navigationName,
                [CanBeNull] MemberInfo memberInfo);

            public abstract IConventionPropertyBuilder OnPropertyAdded([NotNull] IConventionPropertyBuilder propertyBuilder);

            public abstract IConventionAnnotation OnPropertyAnnotationChanged(
                [NotNull] IConventionPropertyBuilder propertyBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation);

            public abstract FieldInfo OnPropertyFieldChanged(
                [NotNull] IConventionPropertyBuilder propertyBuilder, FieldInfo newFieldInfo, [CanBeNull] FieldInfo oldFieldInfo);

            public abstract IConventionPropertyBuilder OnPropertyNullableChanged([NotNull] IConventionPropertyBuilder propertyBuilder);
        }
    }
}
