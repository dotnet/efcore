// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

public partial class ConventionDispatcher
{
    private sealed class DelayedConventionScope(ConventionScope parent, List<ConventionNode>? children = null) : ConventionScope
    {
        public override ConventionScope Parent { [DebuggerStepThrough] get; } = parent;

        public override IReadOnlyList<ConventionNode>? Children
        {
            [DebuggerStepThrough]
            get => children;
        }

        private void Add(ConventionNode node)
        {
            children ??= [];

            children.Add(node);
        }

        public override void Run(ConventionDispatcher dispatcher)
        {
            if (children == null)
            {
                return;
            }

            foreach (var conventionNode in children)
            {
                conventionNode.Run(dispatcher);
            }
        }

        public override IConventionAnnotation? OnModelAnnotationChanged(
            IConventionModelBuilder modelBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            Add(new OnModelAnnotationChangedNode(modelBuilder, name, annotation, oldAnnotation));
            return annotation;
        }

        public override string? OnModelEmbeddedDiscriminatorNameChanged(
            IConventionModelBuilder modelBuilder,
            string? oldName,
            string? newName)
        {
            Add(new OnModelEmbeddedDiscriminatorNameChangedNode(modelBuilder, oldName, newName));
            return newName;
        }

        public override string OnTypeIgnored(IConventionModelBuilder modelBuilder, string name, Type? type)
        {
            Add(new OnTypeIgnoredNode(modelBuilder, name, type));
            return name;
        }

        public override IConventionEntityTypeBuilder OnEntityTypeAdded(IConventionEntityTypeBuilder entityTypeBuilder)
        {
            Add(new OnEntityTypeAddedNode(entityTypeBuilder));
            return entityTypeBuilder;
        }

        public override IConventionEntityType OnEntityTypeRemoved(
            IConventionModelBuilder modelBuilder,
            IConventionEntityType entityType)
        {
            Add(new OnEntityTypeRemovedNode(modelBuilder, entityType));
            return entityType;
        }

        public override string OnEntityTypeMemberIgnored(IConventionEntityTypeBuilder entityTypeBuilder, string name)
        {
            Add(new OnEntityTypeMemberIgnoredNode(entityTypeBuilder, name));
            return name;
        }

        public override string? OnDiscriminatorPropertySet(IConventionTypeBaseBuilder structuralTypeBuilder, string? name)
        {
            Add(new OnDiscriminatorPropertySetNode(structuralTypeBuilder, name));
            return name;
        }

        public override IConventionEntityType? OnEntityTypeBaseTypeChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionEntityType? newBaseType,
            IConventionEntityType? previousBaseType)
        {
            Add(new OnEntityTypeBaseTypeChangedNode(entityTypeBuilder, newBaseType, previousBaseType));
            return newBaseType;
        }

        public override IConventionAnnotation? OnEntityTypeAnnotationChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            Add(new OnEntityTypeAnnotationChangedNode(entityTypeBuilder, name, annotation, oldAnnotation));
            return annotation;
        }

        public override string OnComplexTypeMemberIgnored(IConventionComplexTypeBuilder complexTypeBuilder, string name)
        {
            Add(new OnComplexTypeMemberIgnoredNode(complexTypeBuilder, name));
            return name;
        }

        public override IConventionAnnotation? OnComplexTypeAnnotationChanged(
            IConventionComplexTypeBuilder complexTypeBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            Add(new OnComplexTypeAnnotationChangedNode(complexTypeBuilder, name, annotation, oldAnnotation));
            return annotation;
        }

        public override IConventionComplexPropertyBuilder OnComplexPropertyAdded(IConventionComplexPropertyBuilder propertyBuilder)
        {
            Add(new OnComplexPropertyAddedNode(propertyBuilder));
            return propertyBuilder;
        }

        public override IConventionComplexProperty OnComplexPropertyRemoved(
            IConventionTypeBaseBuilder typeBaseBuilder,
            IConventionComplexProperty property)
        {
            Add(new OnComplexPropertyRemovedNode(typeBaseBuilder, property));
            return property;
        }

        public override bool? OnComplexPropertyNullabilityChanged(IConventionComplexPropertyBuilder propertyBuilder)
        {
            Add(new OnComplexPropertyNullabilityChangedNode(propertyBuilder));
            return propertyBuilder.Metadata.IsNullable;
        }

        public override FieldInfo? OnComplexPropertyFieldChanged(
            IConventionComplexPropertyBuilder propertyBuilder,
            FieldInfo? newFieldInfo,
            FieldInfo? oldFieldInfo)
        {
            Add(new OnComplexPropertyFieldChangedNode(propertyBuilder, newFieldInfo, oldFieldInfo));
            return newFieldInfo;
        }

        public override IConventionAnnotation? OnComplexPropertyAnnotationChanged(
            IConventionComplexPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            Add(new OnComplexPropertyAnnotationChangedNode(propertyBuilder, name, annotation, oldAnnotation));
            return annotation;
        }

        public override IConventionForeignKeyBuilder OnForeignKeyAdded(IConventionForeignKeyBuilder relationshipBuilder)
        {
            Add(new OnForeignKeyAddedNode(relationshipBuilder));
            return relationshipBuilder;
        }

        public override IConventionForeignKey OnForeignKeyRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionForeignKey foreignKey)
        {
            Add(new OnForeignKeyRemovedNode(entityTypeBuilder, foreignKey));
            return foreignKey;
        }

        public override IConventionNavigation? OnForeignKeyNullNavigationSet(
            IConventionForeignKeyBuilder relationshipBuilder,
            bool pointsToPrincipal)
        {
            Add(new OnForeignKeyNullNavigationSetNode(relationshipBuilder, pointsToPrincipal));
            return null;
        }

        public override IConventionAnnotation? OnForeignKeyAnnotationChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            Add(new OnForeignKeyAnnotationChangedNode(relationshipBuilder, name, annotation, oldAnnotation));
            return annotation;
        }

        public override IConventionKeyBuilder OnKeyAdded(IConventionKeyBuilder keyBuilder)
        {
            Add(new OnKeyAddedNode(keyBuilder));
            return keyBuilder;
        }

        public override IConventionKey OnKeyRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionKey key)
        {
            Add(new OnKeyRemovedNode(entityTypeBuilder, key));
            return key;
        }

        public override IConventionAnnotation? OnKeyAnnotationChanged(
            IConventionKeyBuilder keyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            Add(new OnKeyAnnotationChangedNode(keyBuilder, name, annotation, oldAnnotation));
            return annotation;
        }

        public override IConventionKey? OnEntityTypePrimaryKeyChanged(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionKey? newPrimaryKey,
            IConventionKey? previousPrimaryKey)
        {
            Add(new OnEntityTypePrimaryKeyChangedNode(entityTypeBuilder, newPrimaryKey, previousPrimaryKey));
            return newPrimaryKey;
        }

        public override IConventionIndexBuilder OnIndexAdded(IConventionIndexBuilder indexBuilder)
        {
            Add(new OnIndexAddedNode(indexBuilder));
            return indexBuilder;
        }

        public override IConventionIndex OnIndexRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionIndex index)
        {
            Add(new OnIndexRemovedNode(entityTypeBuilder, index));
            return index;
        }

        public override bool? OnIndexUniquenessChanged(IConventionIndexBuilder indexBuilder)
        {
            Add(new OnIndexUniquenessChangedNode(indexBuilder));
            return indexBuilder.Metadata.IsUnique;
        }

        public override IReadOnlyList<bool>? OnIndexSortOrderChanged(IConventionIndexBuilder indexBuilder)
        {
            Add(new OnIndexSortOrderChangedNode(indexBuilder));
            return indexBuilder.Metadata.IsDescending;
        }

        public override IConventionAnnotation? OnIndexAnnotationChanged(
            IConventionIndexBuilder indexBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            Add(new OnIndexAnnotationChangedNode(indexBuilder, name, annotation, oldAnnotation));
            return annotation;
        }

        public override IConventionNavigationBuilder OnNavigationAdded(IConventionNavigationBuilder navigationBuilder)
        {
            Add(new OnNavigationAddedNode(navigationBuilder));
            return navigationBuilder;
        }

        public override IConventionAnnotation? OnNavigationAnnotationChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IConventionNavigation navigation,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            Add(new OnNavigationAnnotationChangedNode(relationshipBuilder, navigation, name, annotation, oldAnnotation));
            return annotation;
        }

        public override string OnNavigationRemoved(
            IConventionEntityTypeBuilder sourceEntityTypeBuilder,
            IConventionEntityTypeBuilder targetEntityTypeBuilder,
            string navigationName,
            MemberInfo? memberInfo)
        {
            Add(new OnNavigationRemovedNode(sourceEntityTypeBuilder, targetEntityTypeBuilder, navigationName, memberInfo));
            return navigationName;
        }

        public override IConventionSkipNavigationBuilder OnSkipNavigationAdded(
            IConventionSkipNavigationBuilder navigationBuilder)
        {
            Add(new OnSkipNavigationAddedNode(navigationBuilder));
            return navigationBuilder;
        }

        public override IConventionAnnotation? OnSkipNavigationAnnotationChanged(
            IConventionSkipNavigationBuilder navigationBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            Add(new OnSkipNavigationAnnotationChangedNode(navigationBuilder, name, annotation, oldAnnotation));
            return annotation;
        }

        public override IConventionForeignKey? OnSkipNavigationForeignKeyChanged(
            IConventionSkipNavigationBuilder navigationBuilder,
            IConventionForeignKey? foreignKey,
            IConventionForeignKey? oldForeignKey)
        {
            Add(new OnSkipNavigationForeignKeyChangedNode(navigationBuilder, foreignKey, oldForeignKey));
            return foreignKey;
        }

        public override IConventionSkipNavigation? OnSkipNavigationInverseChanged(
            IConventionSkipNavigationBuilder navigationBuilder,
            IConventionSkipNavigation? inverse,
            IConventionSkipNavigation? oldInverse)
        {
            Add(new OnSkipNavigationInverseChangedNode(navigationBuilder, inverse, oldInverse));
            return inverse;
        }

        public override IConventionSkipNavigation OnSkipNavigationRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionSkipNavigation navigation)
        {
            Add(new OnSkipNavigationRemovedNode(entityTypeBuilder, navigation));
            return navigation;
        }

        public override IConventionTriggerBuilder OnTriggerAdded(
            IConventionTriggerBuilder navigationBuilder)
        {
            Add(new OnTriggerAddedNode(navigationBuilder));
            return navigationBuilder;
        }

        public override IConventionTrigger OnTriggerRemoved(
            IConventionEntityTypeBuilder entityTypeBuilder,
            IConventionTrigger navigation)
        {
            Add(new OnTriggerRemovedNode(entityTypeBuilder, navigation));
            return navigation;
        }

        public override IReadOnlyList<IConventionProperty> OnForeignKeyPropertiesChanged(
            IConventionForeignKeyBuilder relationshipBuilder,
            IReadOnlyList<IConventionProperty> oldDependentProperties,
            IConventionKey oldPrincipalKey)
        {
            Add(new OnForeignKeyPropertiesChangedNode(relationshipBuilder, oldDependentProperties, oldPrincipalKey));
            return relationshipBuilder.Metadata.Properties;
        }

        public override bool? OnForeignKeyUniquenessChanged(
            IConventionForeignKeyBuilder relationshipBuilder)
        {
            Add(new OnForeignKeyUniquenessChangedNode(relationshipBuilder));
            return relationshipBuilder.Metadata.IsUnique;
        }

        public override bool? OnForeignKeyRequirednessChanged(
            IConventionForeignKeyBuilder relationshipBuilder)
        {
            Add(new OnForeignKeyRequirednessChangedNode(relationshipBuilder));
            return relationshipBuilder.Metadata.IsRequired;
        }

        public override bool? OnForeignKeyDependentRequirednessChanged(
            IConventionForeignKeyBuilder relationshipBuilder)
        {
            Add(new OnForeignKeyDependentRequirednessChangedNode(relationshipBuilder));
            return relationshipBuilder.Metadata.IsRequiredDependent;
        }

        public override bool? OnForeignKeyOwnershipChanged(
            IConventionForeignKeyBuilder relationshipBuilder)
        {
            Add(new OnForeignKeyOwnershipChangedNode(relationshipBuilder));
            return relationshipBuilder.Metadata.IsOwnership;
        }

        public override IConventionForeignKeyBuilder OnForeignKeyPrincipalEndChanged(
            IConventionForeignKeyBuilder relationshipBuilder)
        {
            Add(new OnForeignKeyPrincipalEndChangedNode(relationshipBuilder));
            return relationshipBuilder;
        }

        public override IConventionPropertyBuilder OnPropertyAdded(IConventionPropertyBuilder propertyBuilder)
        {
            Add(new OnPropertyAddedNode(propertyBuilder));
            return propertyBuilder;
        }

        public override bool? OnPropertyNullabilityChanged(IConventionPropertyBuilder propertyBuilder)
        {
            Add(new OnPropertyNullabilityChangedNode(propertyBuilder));
            return propertyBuilder.Metadata.IsNullable;
        }

        public override bool? OnElementTypeNullabilityChanged(IConventionElementTypeBuilder builder)
        {
            Add(new OnElementTypeNullabilityChangedNode(builder));
            return builder.Metadata.IsNullable;
        }

        public override FieldInfo? OnPropertyFieldChanged(
            IConventionPropertyBuilder propertyBuilder,
            FieldInfo? newFieldInfo,
            FieldInfo? oldFieldInfo)
        {
            Add(new OnPropertyFieldChangedNode(propertyBuilder, newFieldInfo, oldFieldInfo));
            return newFieldInfo;
        }

        public override IConventionAnnotation? OnPropertyAnnotationChanged(
            IConventionPropertyBuilder propertyBuilder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            Add(new OnPropertyAnnotationChangedNode(propertyBuilder, name, annotation, oldAnnotation));
            return annotation;
        }

        public override IConventionAnnotation? OnElementTypeAnnotationChanged(
            IConventionElementTypeBuilder builder,
            string name,
            IConventionAnnotation? annotation,
            IConventionAnnotation? oldAnnotation)
        {
            Add(new OnElementTypeAnnotationChangedNode(builder, name, annotation, oldAnnotation));
            return annotation;
        }

        public override IConventionProperty OnPropertyRemoved(
            IConventionTypeBaseBuilder typeBaseBuilder,
            IConventionProperty property)
        {
            Add(new OnPropertyRemovedNode(typeBaseBuilder, property));
            return property;
        }

        public override IElementType? OnPropertyElementTypeChanged(
            IConventionPropertyBuilder propertyBuilder,
            IElementType? newElementType,
            IElementType? oldElementType)
        {
            Add(new OnPropertyElementTypeChangedNode(propertyBuilder, newElementType, oldElementType));
            return newElementType;
        }
    }

    private sealed class OnModelAnnotationChangedNode(
        IConventionModelBuilder modelBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        : ConventionNode
    {
        public IConventionModelBuilder ModelBuilder { get; } = modelBuilder;
        public string Name { get; } = name;
        public IConventionAnnotation? Annotation { get; } = annotation;
        public IConventionAnnotation? OldAnnotation { get; } = oldAnnotation;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnModelAnnotationChanged(
                ModelBuilder, Name, Annotation, OldAnnotation);
    }

    private sealed class OnModelEmbeddedDiscriminatorNameChangedNode(
        IConventionModelBuilder modelBuilder,
        string? oldName,
        string? newName)
        : ConventionNode
    {
        public IConventionModelBuilder ModelBuilder { get; } = modelBuilder;
        public string? OldName { get; } = oldName;
        public string? NewName { get; } = newName;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnModelEmbeddedDiscriminatorNameChanged(ModelBuilder, OldName, NewName);
    }

    private sealed class OnTypeIgnoredNode(IConventionModelBuilder modelBuilder, string name, Type? type) : ConventionNode
    {
        public IConventionModelBuilder ModelBuilder { get; } = modelBuilder;
        public string Name { get; } = name;
        public Type? Type { get; } = type;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnTypeIgnored(ModelBuilder, Name, Type);
    }

    private sealed class OnEntityTypeAddedNode(IConventionEntityTypeBuilder entityTypeBuilder) : ConventionNode
    {
        public IConventionEntityTypeBuilder EntityTypeBuilder { get; } = entityTypeBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnEntityTypeAdded(EntityTypeBuilder);
    }

    private sealed class OnEntityTypeRemovedNode(IConventionModelBuilder modelBuilder, IConventionEntityType entityType)
        : ConventionNode
    {
        public IConventionModelBuilder ModelBuilder { get; } = modelBuilder;
        public IConventionEntityType EntityType { get; } = entityType;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnEntityTypeRemoved(ModelBuilder, EntityType);
    }

    private sealed class OnEntityTypeMemberIgnoredNode(IConventionEntityTypeBuilder entityTypeBuilder, string name) : ConventionNode
    {
        public IConventionEntityTypeBuilder EntityTypeBuilder { get; } = entityTypeBuilder;
        public string Name { get; } = name;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnEntityTypeMemberIgnored(EntityTypeBuilder, Name);
    }

    private sealed class OnDiscriminatorPropertySetNode(IConventionTypeBaseBuilder structuralTypeBuilder, string? name) : ConventionNode
    {
        public IConventionTypeBaseBuilder StructuralTypeBuilder { get; } = structuralTypeBuilder;
        public string? Name { get; } = name;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnDiscriminatorPropertySet(StructuralTypeBuilder, Name);
    }

    private sealed class OnEntityTypeBaseTypeChangedNode(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? previousBaseType)
        : ConventionNode
    {
        public IConventionEntityTypeBuilder EntityTypeBuilder { get; } = entityTypeBuilder;
        public IConventionEntityType? NewBaseType { get; } = newBaseType;
        public IConventionEntityType? PreviousBaseType { get; } = previousBaseType;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnEntityTypeBaseTypeChanged(
                EntityTypeBuilder, NewBaseType, PreviousBaseType);
    }

    private sealed class OnEntityTypeAnnotationChangedNode(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        : ConventionNode
    {
        public IConventionEntityTypeBuilder EntityTypeBuilder { get; } = entityTypeBuilder;
        public string Name { get; } = name;
        public IConventionAnnotation? Annotation { get; } = annotation;
        public IConventionAnnotation? OldAnnotation { get; } = oldAnnotation;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnEntityTypeAnnotationChanged(
                EntityTypeBuilder, Name, Annotation, OldAnnotation);
    }

    private sealed class OnComplexTypeMemberIgnoredNode(IConventionComplexTypeBuilder complexTypeBuilder, string name) : ConventionNode
    {
        public IConventionComplexTypeBuilder ComplexTypeBuilder { get; } = complexTypeBuilder;
        public string Name { get; } = name;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnComplexTypeMemberIgnored(ComplexTypeBuilder, Name);
    }

    private sealed class OnComplexTypeAnnotationChangedNode(
        IConventionComplexTypeBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        : ConventionNode
    {
        public IConventionComplexTypeBuilder ComplexTypeBuilder { get; } = propertyBuilder;
        public string Name { get; } = name;
        public IConventionAnnotation? Annotation { get; } = annotation;
        public IConventionAnnotation? OldAnnotation { get; } = oldAnnotation;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnComplexTypeAnnotationChanged(
                ComplexTypeBuilder, Name, Annotation, OldAnnotation);
    }

    private sealed class OnComplexPropertyAddedNode(IConventionComplexPropertyBuilder propertyBuilder) : ConventionNode
    {
        public IConventionComplexPropertyBuilder PropertyBuilder { get; } = propertyBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnComplexPropertyAdded(PropertyBuilder);
    }

    private sealed class OnComplexPropertyRemovedNode(IConventionTypeBaseBuilder modelBuilder, IConventionComplexProperty entityType)
        : ConventionNode
    {
        public IConventionTypeBaseBuilder TypeBaseBuilder { get; } = modelBuilder;
        public IConventionComplexProperty ComplexProperty { get; } = entityType;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnComplexPropertyRemoved(TypeBaseBuilder, ComplexProperty);
    }

    private sealed class OnComplexPropertyNullabilityChangedNode(IConventionComplexPropertyBuilder propertyBuilder) : ConventionNode
    {
        public IConventionComplexPropertyBuilder PropertyBuilder { get; } = propertyBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnComplexPropertyNullabilityChanged(PropertyBuilder);
    }

    private sealed class OnComplexPropertyFieldChangedNode(
        IConventionComplexPropertyBuilder propertyBuilder,
        FieldInfo? newFieldInfo,
        FieldInfo? oldFieldInfo)
        : ConventionNode
    {
        public IConventionComplexPropertyBuilder PropertyBuilder { get; } = propertyBuilder;
        public FieldInfo? NewFieldInfo { get; } = newFieldInfo;
        public FieldInfo? OldFieldInfo { get; } = oldFieldInfo;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnComplexPropertyFieldChanged(PropertyBuilder, NewFieldInfo, OldFieldInfo);
    }

    private sealed class OnComplexPropertyAnnotationChangedNode(
        IConventionComplexPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        : ConventionNode
    {
        public IConventionComplexPropertyBuilder PropertyBuilder { get; } = propertyBuilder;
        public string Name { get; } = name;
        public IConventionAnnotation? Annotation { get; } = annotation;
        public IConventionAnnotation? OldAnnotation { get; } = oldAnnotation;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnComplexPropertyAnnotationChanged(
                PropertyBuilder, Name, Annotation, OldAnnotation);
    }

    private sealed class OnForeignKeyAddedNode(IConventionForeignKeyBuilder relationshipBuilder) : ConventionNode
    {
        public IConventionForeignKeyBuilder RelationshipBuilder { get; } = relationshipBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnForeignKeyAdded(RelationshipBuilder);
    }

    private sealed class OnForeignKeyRemovedNode(IConventionEntityTypeBuilder entityTypeBuilder, IConventionForeignKey foreignKey)
        : ConventionNode
    {
        public IConventionEntityTypeBuilder EntityTypeBuilder { get; } = entityTypeBuilder;
        public IConventionForeignKey ForeignKey { get; } = foreignKey;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnForeignKeyRemoved(EntityTypeBuilder, ForeignKey);
    }

    private sealed class OnForeignKeyAnnotationChangedNode(
        IConventionForeignKeyBuilder relationshipBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        : ConventionNode
    {
        public IConventionForeignKeyBuilder RelationshipBuilder { get; } = relationshipBuilder;
        public string Name { get; } = name;
        public IConventionAnnotation? Annotation { get; } = annotation;
        public IConventionAnnotation? OldAnnotation { get; } = oldAnnotation;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnForeignKeyAnnotationChanged(
                RelationshipBuilder, Name, Annotation, OldAnnotation);
    }

    private sealed class OnForeignKeyPropertiesChangedNode(
        IConventionForeignKeyBuilder relationshipBuilder,
        IReadOnlyList<IConventionProperty> oldDependentProperties,
        IConventionKey oldPrincipalKey)
        : ConventionNode
    {
        public IConventionForeignKeyBuilder RelationshipBuilder { get; } = relationshipBuilder;
        public IReadOnlyList<IConventionProperty> OldDependentProperties { get; } = oldDependentProperties;
        public IConventionKey OldPrincipalKey { get; } = oldPrincipalKey;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnForeignKeyPropertiesChanged(
                RelationshipBuilder, OldDependentProperties, OldPrincipalKey);
    }

    private sealed class OnForeignKeyUniquenessChangedNode(IConventionForeignKeyBuilder relationshipBuilder) : ConventionNode
    {
        public IConventionForeignKeyBuilder RelationshipBuilder { get; } = relationshipBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnForeignKeyUniquenessChanged(RelationshipBuilder);
    }

    private sealed class OnForeignKeyRequirednessChangedNode(IConventionForeignKeyBuilder relationshipBuilder) : ConventionNode
    {
        public IConventionForeignKeyBuilder RelationshipBuilder { get; } = relationshipBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnForeignKeyRequirednessChanged(RelationshipBuilder);
    }

    private sealed class OnForeignKeyDependentRequirednessChangedNode(IConventionForeignKeyBuilder relationshipBuilder) : ConventionNode
    {
        public IConventionForeignKeyBuilder RelationshipBuilder { get; } = relationshipBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnForeignKeyDependentRequirednessChanged(RelationshipBuilder);
    }

    private sealed class OnForeignKeyOwnershipChangedNode(IConventionForeignKeyBuilder relationshipBuilder) : ConventionNode
    {
        public IConventionForeignKeyBuilder RelationshipBuilder { get; } = relationshipBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnForeignKeyOwnershipChanged(RelationshipBuilder);
    }

    private sealed class OnForeignKeyNullNavigationSetNode(IConventionForeignKeyBuilder relationshipBuilder, bool pointsToPrincipal)
        : ConventionNode
    {
        public IConventionForeignKeyBuilder RelationshipBuilder { get; } = relationshipBuilder;
        public bool PointsToPrincipal { get; } = pointsToPrincipal;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnForeignKeyNullNavigationSet(RelationshipBuilder, PointsToPrincipal);
    }

    private sealed class OnForeignKeyPrincipalEndChangedNode(IConventionForeignKeyBuilder relationshipBuilder) : ConventionNode
    {
        public IConventionForeignKeyBuilder RelationshipBuilder { get; } = relationshipBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnForeignKeyPrincipalEndChanged(RelationshipBuilder);
    }

    private sealed class OnNavigationAddedNode(IConventionNavigationBuilder navigationBuilder) : ConventionNode
    {
        public IConventionNavigationBuilder NavigationBuilder { get; } = navigationBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnNavigationAdded(NavigationBuilder);
    }

    private sealed class OnNavigationAnnotationChangedNode(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionNavigation navigation,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        : ConventionNode
    {
        public IConventionForeignKeyBuilder RelationshipBuilder { get; } = relationshipBuilder;
        public IConventionNavigation Navigation { get; } = navigation;
        public string Name { get; } = name;
        public IConventionAnnotation? Annotation { get; } = annotation;
        public IConventionAnnotation? OldAnnotation { get; } = oldAnnotation;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnNavigationAnnotationChanged(
                RelationshipBuilder, Navigation, Name, Annotation, OldAnnotation);
    }

    private sealed class OnNavigationRemovedNode(
        IConventionEntityTypeBuilder sourceEntityTypeBuilder,
        IConventionEntityTypeBuilder targetEntityTypeBuilder,
        string navigationName,
        MemberInfo? memberInfo)
        : ConventionNode
    {
        public IConventionEntityTypeBuilder SourceEntityTypeBuilder { get; } = sourceEntityTypeBuilder;
        public IConventionEntityTypeBuilder TargetEntityTypeBuilder { get; } = targetEntityTypeBuilder;
        public string NavigationName { get; } = navigationName;
        public MemberInfo? MemberInfo { get; } = memberInfo;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnNavigationRemoved(
                SourceEntityTypeBuilder, TargetEntityTypeBuilder, NavigationName, MemberInfo);
    }

    private sealed class OnSkipNavigationAddedNode(IConventionSkipNavigationBuilder navigationBuilder) : ConventionNode
    {
        public IConventionSkipNavigationBuilder NavigationBuilder { get; } = navigationBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnSkipNavigationAdded(NavigationBuilder);
    }

    private sealed class OnSkipNavigationAnnotationChangedNode(
        IConventionSkipNavigationBuilder navigationBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        : ConventionNode
    {
        public IConventionSkipNavigationBuilder NavigationBuilder { get; } = navigationBuilder;
        public string Name { get; } = name;
        public IConventionAnnotation? Annotation { get; } = annotation;
        public IConventionAnnotation? OldAnnotation { get; } = oldAnnotation;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnSkipNavigationAnnotationChanged(
                NavigationBuilder, Name, Annotation, OldAnnotation);
    }

    private sealed class OnSkipNavigationForeignKeyChangedNode(
        IConventionSkipNavigationBuilder navigationBuilder,
        IConventionForeignKey? foreignKey,
        IConventionForeignKey? oldForeignKey)
        : ConventionNode
    {
        public IConventionSkipNavigationBuilder NavigationBuilder { get; } = navigationBuilder;
        public IConventionForeignKey? ForeignKey { get; } = foreignKey;
        public IConventionForeignKey? OldForeignKey { get; } = oldForeignKey;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnSkipNavigationForeignKeyChanged(NavigationBuilder, ForeignKey, OldForeignKey);
    }

    private sealed class OnSkipNavigationInverseChangedNode(
        IConventionSkipNavigationBuilder navigationBuilder,
        IConventionSkipNavigation? inverse,
        IConventionSkipNavigation? oldInverse)
        : ConventionNode
    {
        public IConventionSkipNavigationBuilder NavigationBuilder { get; } = navigationBuilder;
        public IConventionSkipNavigation? Inverse { get; } = inverse;
        public IConventionSkipNavigation? OldInverse { get; } = oldInverse;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnSkipNavigationInverseChanged(NavigationBuilder, Inverse, OldInverse);
    }

    private sealed class OnSkipNavigationRemovedNode(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionSkipNavigation navigation)
        : ConventionNode
    {
        public IConventionEntityTypeBuilder EntityTypeBuilder { get; } = entityTypeBuilder;
        public IConventionSkipNavigation Navigation { get; } = navigation;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnSkipNavigationRemoved(EntityTypeBuilder, Navigation);
    }

    private sealed class OnTriggerAddedNode(IConventionTriggerBuilder triggerBuilder) : ConventionNode
    {
        public IConventionTriggerBuilder TriggerBuilder { get; } = triggerBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnTriggerAdded(TriggerBuilder);
    }

    private sealed class OnTriggerRemovedNode(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionTrigger trigger)
        : ConventionNode
    {
        public IConventionEntityTypeBuilder EntityTypeBuilder { get; } = entityTypeBuilder;
        public IConventionTrigger Trigger { get; } = trigger;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnTriggerRemoved(EntityTypeBuilder, Trigger);
    }

    private sealed class OnKeyAddedNode(IConventionKeyBuilder keyBuilder) : ConventionNode
    {
        public IConventionKeyBuilder KeyBuilder { get; } = keyBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnKeyAdded(KeyBuilder);
    }

    private sealed class OnKeyRemovedNode(IConventionEntityTypeBuilder entityTypeBuilder, IConventionKey key) : ConventionNode
    {
        public IConventionEntityTypeBuilder EntityTypeBuilder { get; } = entityTypeBuilder;
        public IConventionKey Key { get; } = key;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnKeyRemoved(EntityTypeBuilder, Key);
    }

    private sealed class OnKeyAnnotationChangedNode(
        IConventionKeyBuilder keyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        : ConventionNode
    {
        public IConventionKeyBuilder KeyBuilder { get; } = keyBuilder;
        public string Name { get; } = name;
        public IConventionAnnotation? Annotation { get; } = annotation;
        public IConventionAnnotation? OldAnnotation { get; } = oldAnnotation;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnKeyAnnotationChanged(
                KeyBuilder, Name, Annotation, OldAnnotation);
    }

    private sealed class OnEntityTypePrimaryKeyChangedNode(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionKey? newPrimaryKey,
        IConventionKey? previousPrimaryKey)
        : ConventionNode
    {
        public IConventionEntityTypeBuilder EntityTypeBuilder { get; } = entityTypeBuilder;
        public IConventionKey? NewPrimaryKey { get; } = newPrimaryKey;
        public IConventionKey? PreviousPrimaryKey { get; } = previousPrimaryKey;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnEntityTypePrimaryKeyChanged(
                EntityTypeBuilder, NewPrimaryKey, PreviousPrimaryKey);
    }

    private sealed class OnIndexAddedNode(IConventionIndexBuilder indexBuilder) : ConventionNode
    {
        public IConventionIndexBuilder IndexBuilder { get; } = indexBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnIndexAdded(IndexBuilder);
    }

    private sealed class OnIndexRemovedNode(IConventionEntityTypeBuilder entityTypeBuilder, IConventionIndex index)
        : ConventionNode
    {
        public IConventionEntityTypeBuilder EntityTypeBuilder { get; } = entityTypeBuilder;
        public IConventionIndex Index { get; } = index;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnIndexRemoved(EntityTypeBuilder, Index);
    }

    private sealed class OnIndexUniquenessChangedNode(IConventionIndexBuilder indexBuilder) : ConventionNode
    {
        public IConventionIndexBuilder IndexBuilder { get; } = indexBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnIndexUniquenessChanged(IndexBuilder);
    }

    private sealed class OnIndexSortOrderChangedNode(IConventionIndexBuilder indexBuilder) : ConventionNode
    {
        public IConventionIndexBuilder IndexBuilder { get; } = indexBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnIndexSortOrderChanged(IndexBuilder);
    }

    private sealed class OnIndexAnnotationChangedNode(
        IConventionIndexBuilder indexBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        : ConventionNode
    {
        public IConventionIndexBuilder IndexBuilder { get; } = indexBuilder;
        public string Name { get; } = name;
        public IConventionAnnotation? Annotation { get; } = annotation;
        public IConventionAnnotation? OldAnnotation { get; } = oldAnnotation;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnIndexAnnotationChanged(
                IndexBuilder, Name, Annotation, OldAnnotation);
    }

    private sealed class OnPropertyAddedNode(IConventionPropertyBuilder propertyBuilder) : ConventionNode
    {
        public IConventionPropertyBuilder PropertyBuilder { get; } = propertyBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnPropertyAdded(PropertyBuilder);
    }

    private sealed class OnPropertyNullabilityChangedNode(IConventionPropertyBuilder propertyBuilder) : ConventionNode
    {
        public IConventionPropertyBuilder PropertyBuilder { get; } = propertyBuilder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnPropertyNullabilityChanged(PropertyBuilder);
    }

    private sealed class OnElementTypeNullabilityChangedNode(IConventionElementTypeBuilder builder) : ConventionNode
    {
        public IConventionElementTypeBuilder ElementTypeBuilder { get; } = builder;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnElementTypeNullabilityChanged(ElementTypeBuilder);
    }

    private sealed class OnPropertyFieldChangedNode(
        IConventionPropertyBuilder propertyBuilder,
        FieldInfo? newFieldInfo,
        FieldInfo? oldFieldInfo)
        : ConventionNode
    {
        public IConventionPropertyBuilder PropertyBuilder { get; } = propertyBuilder;
        public FieldInfo? NewFieldInfo { get; } = newFieldInfo;
        public FieldInfo? OldFieldInfo { get; } = oldFieldInfo;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnPropertyFieldChanged(PropertyBuilder, NewFieldInfo, OldFieldInfo);
    }

    private sealed class OnPropertyElementTypeChangedNode(
        IConventionPropertyBuilder propertyBuilder,
        IElementType? newElementType,
        IElementType? oldElementType)
        : ConventionNode
    {
        public IConventionPropertyBuilder PropertyBuilder { get; } = propertyBuilder;
        public IElementType? NewElementType { get; } = newElementType;
        public IElementType? OldElementType { get; } = oldElementType;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnPropertyElementTypeChanged(PropertyBuilder, NewElementType, OldElementType);
    }

    private sealed class OnPropertyAnnotationChangedNode(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        : ConventionNode
    {
        public IConventionPropertyBuilder PropertyBuilder { get; } = propertyBuilder;
        public string Name { get; } = name;
        public IConventionAnnotation? Annotation { get; } = annotation;
        public IConventionAnnotation? OldAnnotation { get; } = oldAnnotation;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnPropertyAnnotationChanged(
                PropertyBuilder, Name, Annotation, OldAnnotation);
    }

    private sealed class OnElementTypeAnnotationChangedNode(
        IConventionElementTypeBuilder elementTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation)
        : ConventionNode
    {
        public IConventionElementTypeBuilder ElementTypeBuilder { get; } = elementTypeBuilder;
        public string Name { get; } = name;
        public IConventionAnnotation? Annotation { get; } = annotation;
        public IConventionAnnotation? OldAnnotation { get; } = oldAnnotation;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnElementTypeAnnotationChanged(
                ElementTypeBuilder, Name, Annotation, OldAnnotation);
    }

    private sealed class OnPropertyRemovedNode(
        IConventionTypeBaseBuilder typeBaseBuilder,
        IConventionProperty property)
        : ConventionNode
    {
        public IConventionTypeBaseBuilder TypeBaseBuilder { get; } = typeBaseBuilder;
        public IConventionProperty Property { get; } = property;

        public override void Run(ConventionDispatcher dispatcher)
            => dispatcher._immediateConventionScope.OnPropertyRemoved(TypeBaseBuilder, Property);
    }
}
