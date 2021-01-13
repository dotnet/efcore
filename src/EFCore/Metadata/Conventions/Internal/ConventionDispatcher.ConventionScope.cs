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
            public virtual ConventionScope Parent
                => null;

            public virtual IReadOnlyList<ConventionNode> Children
                => null;

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
                [NotNull] IConventionModelBuilder modelBuilder,
                [NotNull] string name,
                [CanBeNull] Type type);

            public abstract string OnEntityTypeMemberIgnored(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [NotNull] string name);

            public abstract IConventionKey OnEntityTypePrimaryKeyChanged(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [CanBeNull] IConventionKey newPrimaryKey,
                [CanBeNull] IConventionKey previousPrimaryKey);

            public abstract IConventionEntityType OnEntityTypeRemoved(
                [NotNull] IConventionModelBuilder modelBuilder,
                [NotNull] IConventionEntityType entityType);

            public abstract IConventionForeignKeyBuilder OnForeignKeyAdded([NotNull] IConventionForeignKeyBuilder relationshipBuilder);

            public abstract IConventionAnnotation OnForeignKeyAnnotationChanged(
                [NotNull] IConventionForeignKeyBuilder relationshipBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation);

            public abstract bool? OnForeignKeyOwnershipChanged(
                [NotNull] IConventionForeignKeyBuilder relationshipBuilder);

            public abstract IConventionForeignKeyBuilder OnForeignKeyPrincipalEndChanged(
                [NotNull] IConventionForeignKeyBuilder relationshipBuilder);

            public abstract IReadOnlyList<IConventionProperty> OnForeignKeyPropertiesChanged(
                [NotNull] IConventionForeignKeyBuilder relationshipBuilder,
                [NotNull] IReadOnlyList<IConventionProperty> oldDependentProperties,
                [NotNull] IConventionKey oldPrincipalKey);

            public abstract IConventionForeignKey OnForeignKeyRemoved(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [NotNull] IConventionForeignKey foreignKey);

            public abstract bool? OnForeignKeyRequirednessChanged(
                [NotNull] IConventionForeignKeyBuilder relationshipBuilder);

            public abstract bool? OnForeignKeyDependentRequirednessChanged(
                [NotNull] IConventionForeignKeyBuilder relationshipBuilder);

            public abstract bool? OnForeignKeyUniquenessChanged(
                [NotNull] IConventionForeignKeyBuilder relationshipBuilder);

            public abstract IConventionIndexBuilder OnIndexAdded([NotNull] IConventionIndexBuilder indexBuilder);

            public abstract IConventionAnnotation OnIndexAnnotationChanged(
                [NotNull] IConventionIndexBuilder indexBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation);

            public abstract IConventionIndex OnIndexRemoved(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [NotNull] IConventionIndex index);

            public abstract bool? OnIndexUniquenessChanged([NotNull] IConventionIndexBuilder indexBuilder);
            public abstract IConventionKeyBuilder OnKeyAdded([NotNull] IConventionKeyBuilder keyBuilder);

            public abstract IConventionAnnotation OnKeyAnnotationChanged(
                [NotNull] IConventionKeyBuilder keyBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation);

            public abstract IConventionKey OnKeyRemoved(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [NotNull] IConventionKey key);

            public abstract IConventionAnnotation OnModelAnnotationChanged(
                [NotNull] IConventionModelBuilder modelBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation);

            public abstract IConventionNavigationBuilder OnNavigationAdded([NotNull] IConventionNavigationBuilder navigationBuilder);

            public abstract string OnNavigationRemoved(
                [NotNull] IConventionEntityTypeBuilder sourceEntityTypeBuilder,
                [NotNull] IConventionEntityTypeBuilder targetEntityTypeBuilder,
                [NotNull] string navigationName,
                [CanBeNull] MemberInfo memberInfo);

            public abstract IConventionAnnotation OnNavigationAnnotationChanged(
                [NotNull] IConventionForeignKeyBuilder relationshipBuilder,
                [NotNull] IConventionNavigation navigation,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation);

            public abstract IConventionSkipNavigationBuilder OnSkipNavigationAdded(
                [NotNull] IConventionSkipNavigationBuilder navigationBuilder);

            public abstract IConventionForeignKey OnSkipNavigationForeignKeyChanged(
                [NotNull] IConventionSkipNavigationBuilder navigationBuilder,
                [NotNull] IConventionForeignKey foreignKey,
                [NotNull] IConventionForeignKey oldForeignKey);

            public abstract IConventionAnnotation OnSkipNavigationAnnotationChanged(
                [NotNull] IConventionSkipNavigationBuilder navigationBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation);

            public abstract IConventionSkipNavigation OnSkipNavigationInverseChanged(
                [NotNull] IConventionSkipNavigationBuilder navigationBuilder,
                [NotNull] IConventionSkipNavigation inverse,
                [NotNull] IConventionSkipNavigation oldInverse);

            public abstract IConventionSkipNavigation OnSkipNavigationRemoved(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [NotNull] IConventionSkipNavigation navigation);

            public abstract IConventionPropertyBuilder OnPropertyAdded([NotNull] IConventionPropertyBuilder propertyBuilder);

            public abstract IConventionAnnotation OnPropertyAnnotationChanged(
                [NotNull] IConventionPropertyBuilder propertyBuilder,
                [NotNull] string name,
                [CanBeNull] IConventionAnnotation annotation,
                [CanBeNull] IConventionAnnotation oldAnnotation);

            public abstract FieldInfo OnPropertyFieldChanged(
                [NotNull] IConventionPropertyBuilder propertyBuilder,
                FieldInfo newFieldInfo,
                [CanBeNull] FieldInfo oldFieldInfo);

            public abstract bool? OnPropertyNullabilityChanged([NotNull] IConventionPropertyBuilder propertyBuilder);

            public abstract IConventionProperty OnPropertyRemoved(
                [NotNull] IConventionEntityTypeBuilder entityTypeBuilder,
                [NotNull] IConventionProperty property);
        }
    }
}
