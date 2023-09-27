// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

public partial class ConventionDispatcher
{
    private abstract class ConventionScope : ConventionNode
    {
        public virtual ConventionScope? Parent
            => null;

        public virtual IReadOnlyList<ConventionNode>? Children
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

        public abstract string? OnTypeIgnored(
            IConventionModelBuilder modelBuilder,
            string name,
            Type? type);

        public abstract IConventionEntityTypeBuilder? OnEntityTypeAdded(IConventionEntityTypeBuilder entityTypeBuilder);

        public abstract IConventionAnnotation? OnEntityTypeAnnotationChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation);

        public abstract IConventionEntityType? OnEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType? newBaseType,
            IConventionEntityType? previousBaseType);

        public abstract string? OnEntityTypeMemberIgnored(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name);

        public abstract string? OnDiscriminatorPropertySet(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string? name);

        public abstract IConventionKey? OnEntityTypePrimaryKeyChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionKey? newPrimaryKey,
            IConventionKey? previousPrimaryKey);

        public abstract IConventionEntityType? OnEntityTypeRemoved(
            IConventionModelBuilder modelBuilder,
            IConventionEntityType entityType);

        public abstract string? OnComplexTypeMemberIgnored(
            IConventionComplexTypeBuilder propertyBuilder,
            string name);

        public abstract IConventionAnnotation? OnComplexTypeAnnotationChanged(
            IConventionComplexTypeBuilder propertyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation);

        public abstract IConventionComplexPropertyBuilder? OnComplexPropertyAdded(
            IConventionComplexPropertyBuilder propertyBuilder);

        public abstract IConventionComplexProperty? OnComplexPropertyRemoved(
            IConventionTypeBaseBuilder typeBaseBuilder,
            IConventionComplexProperty property);

        public abstract FieldInfo? OnComplexPropertyFieldChanged(
            IConventionComplexPropertyBuilder propertyBuilder,
            FieldInfo? newFieldInfo,
            FieldInfo? oldFieldInfo);

        public abstract bool? OnComplexPropertyNullabilityChanged(
            IConventionComplexPropertyBuilder propertyBuilder);

        public abstract IConventionAnnotation? OnComplexPropertyAnnotationChanged(
            IConventionComplexPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation);

        public abstract IConventionForeignKeyBuilder? OnForeignKeyAdded(IConventionForeignKeyBuilder relationshipBuilder);

        public abstract IConventionAnnotation? OnForeignKeyAnnotationChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation);

        public abstract bool? OnForeignKeyOwnershipChanged(
            IConventionForeignKeyBuilder relationshipBuilder);

        public abstract IConventionForeignKeyBuilder? OnForeignKeyPrincipalEndChanged(
            IConventionForeignKeyBuilder relationshipBuilder);

        public abstract IReadOnlyList<IConventionProperty>? OnForeignKeyPropertiesChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IReadOnlyList<IConventionProperty> oldDependentProperties,
            IConventionKey oldPrincipalKey);

        public abstract IConventionForeignKey? OnForeignKeyRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionForeignKey foreignKey);

        public abstract bool? OnForeignKeyRequirednessChanged(
            IConventionForeignKeyBuilder relationshipBuilder);

        public abstract bool? OnForeignKeyDependentRequirednessChanged(
            IConventionForeignKeyBuilder relationshipBuilder);

        public abstract bool? OnForeignKeyUniquenessChanged(
            IConventionForeignKeyBuilder relationshipBuilder);

        public abstract IConventionNavigation? OnForeignKeyNullNavigationSet(
            IConventionForeignKeyBuilder relationshipBuilder,
            bool pointsToPrincipal);

        public abstract IConventionIndexBuilder? OnIndexAdded(IConventionIndexBuilder indexBuilder);

        public abstract IConventionAnnotation? OnIndexAnnotationChanged(
            IConventionIndexBuilder indexBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation);

        public abstract IConventionIndex? OnIndexRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionIndex index);

        public abstract bool? OnIndexUniquenessChanged(IConventionIndexBuilder indexBuilder);
        public abstract IReadOnlyList<bool>? OnIndexSortOrderChanged(IConventionIndexBuilder indexBuilder);

        public abstract IConventionKeyBuilder? OnKeyAdded(IConventionKeyBuilder keyBuilder);

        public abstract IConventionAnnotation? OnKeyAnnotationChanged(
            IConventionKeyBuilder keyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation);

        public abstract IConventionKey? OnKeyRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionKey key);

        public abstract IConventionAnnotation? OnModelAnnotationChanged(
            IConventionModelBuilder modelBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation);

        public abstract IConventionNavigationBuilder? OnNavigationAdded(IConventionNavigationBuilder navigationBuilder);

        public abstract string? OnNavigationRemoved(
            IConventionEntityTypeBuilder sourceEntityTypeBuilder,
            IConventionEntityTypeBuilder targetEntityTypeBuilder,
            string navigationName,
            MemberInfo? memberInfo);

        public abstract IConventionAnnotation? OnNavigationAnnotationChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionNavigation navigation,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation);

        public abstract IConventionSkipNavigationBuilder? OnSkipNavigationAdded(
            IConventionSkipNavigationBuilder navigationBuilder);

        public abstract IConventionForeignKey? OnSkipNavigationForeignKeyChanged(
            IConventionSkipNavigationBuilder navigationBuilder,
            IConventionForeignKey? foreignKey,
            IConventionForeignKey? oldForeignKey);

        public abstract IConventionAnnotation? OnSkipNavigationAnnotationChanged(
            IConventionSkipNavigationBuilder navigationBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation);

        public abstract IConventionSkipNavigation? OnSkipNavigationInverseChanged(
            IConventionSkipNavigationBuilder navigationBuilder,
            IConventionSkipNavigation? inverse,
            IConventionSkipNavigation? oldInverse);

        public abstract IConventionSkipNavigation? OnSkipNavigationRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionSkipNavigation navigation);

        public abstract IConventionPropertyBuilder? OnPropertyAdded(
            IConventionPropertyBuilder propertyBuilder);

        public abstract IConventionAnnotation? OnPropertyAnnotationChanged(
            IConventionPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation);

        public abstract FieldInfo? OnPropertyFieldChanged(
            IConventionPropertyBuilder propertyBuilder,
            FieldInfo? newFieldInfo,
            FieldInfo? oldFieldInfo);

        public abstract bool? OnPropertyNullabilityChanged(
            IConventionPropertyBuilder propertyBuilder);

        public abstract IConventionProperty? OnPropertyRemoved(
            IConventionTypeBaseBuilder typeBaseBuilder,
            IConventionProperty property);

        public abstract IElementType? OnPropertyElementTypeChanged(
            IConventionPropertyBuilder propertyBuilder,
            IElementType? newElementType,
            IElementType? oldElementType);

        public abstract IConventionTriggerBuilder? OnTriggerAdded(IConventionTriggerBuilder triggerBuilder);

        public abstract IConventionTrigger? OnTriggerRemoved(IConventionEntityTypeBuilder entityTypeBuilder, IConventionTrigger trigger);

        public abstract IConventionAnnotation? OnElementTypeAnnotationChanged(
            IConventionElementTypeBuilder builder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation);

        public abstract bool? OnElementTypeNullabilityChanged(
            IConventionElementTypeBuilder builder);
    }
}
