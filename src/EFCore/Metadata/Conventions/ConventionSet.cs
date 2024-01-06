// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Represents a set of conventions used to build a model.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class ConventionSet
{
    /// <summary>
    ///     Conventions to run to setup the initial model.
    /// </summary>
    public virtual List<IModelInitializedConvention> ModelInitializedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when model building is completed.
    /// </summary>
    public virtual List<IModelFinalizingConvention> ModelFinalizingConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when model validation is completed.
    /// </summary>
    public virtual List<IModelFinalizedConvention> ModelFinalizedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an annotation is set or removed on a model.
    /// </summary>
    public virtual List<IModelAnnotationChangedConvention> ModelAnnotationChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a type is ignored.
    /// </summary>
    public virtual List<ITypeIgnoredConvention> TypeIgnoredConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an entity type is added to the model.
    /// </summary>
    public virtual List<IEntityTypeAddedConvention> EntityTypeAddedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an entity type is removed.
    /// </summary>
    public virtual List<IEntityTypeRemovedConvention> EntityTypeRemovedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a property is ignored.
    /// </summary>
    public virtual List<IEntityTypeMemberIgnoredConvention> EntityTypeMemberIgnoredConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a discriminator property is set.
    /// </summary>
    public virtual List<IDiscriminatorPropertySetConvention> DiscriminatorPropertySetConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the base entity type is changed.
    /// </summary>
    public virtual List<IEntityTypeBaseTypeChangedConvention> EntityTypeBaseTypeChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a primary key is changed.
    /// </summary>
    public virtual List<IEntityTypePrimaryKeyChangedConvention> EntityTypePrimaryKeyChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an annotation is set or removed on an entity type.
    /// </summary>
    public virtual List<IEntityTypeAnnotationChangedConvention> EntityTypeAnnotationChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a property is ignored.
    /// </summary>
    public virtual List<IComplexTypeMemberIgnoredConvention> ComplexTypeMemberIgnoredConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an annotation is set or removed on a complex type.
    /// </summary>
    public virtual List<IComplexTypeAnnotationChangedConvention> ComplexTypeAnnotationChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an entity type is added to the model.
    /// </summary>
    public virtual List<IComplexPropertyAddedConvention> ComplexPropertyAddedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an entity type is removed.
    /// </summary>
    public virtual List<IComplexPropertyRemovedConvention> ComplexPropertyRemovedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the nullability of a property is changed.
    /// </summary>
    public virtual List<IComplexPropertyNullabilityChangedConvention> ComplexPropertyNullabilityChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the field of a property is changed.
    /// </summary>
    public virtual List<IComplexPropertyFieldChangedConvention> ComplexPropertyFieldChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an annotation is set or removed on a complex property.
    /// </summary>
    public virtual List<IComplexPropertyAnnotationChangedConvention> ComplexPropertyAnnotationChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a foreign key is added.
    /// </summary>
    public virtual List<IForeignKeyAddedConvention> ForeignKeyAddedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a foreign key is removed.
    /// </summary>
    public virtual List<IForeignKeyRemovedConvention> ForeignKeyRemovedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the principal end of a relationship is configured.
    /// </summary>
    public virtual List<IForeignKeyPrincipalEndChangedConvention> ForeignKeyPrincipalEndChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the properties or the principal key of a foreign key are changed.
    /// </summary>
    public virtual List<IForeignKeyPropertiesChangedConvention> ForeignKeyPropertiesChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the uniqueness of a foreign key is changed.
    /// </summary>
    public virtual List<IForeignKeyUniquenessChangedConvention> ForeignKeyUniquenessChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the requiredness of a foreign key is changed.
    /// </summary>
    public virtual List<IForeignKeyRequirednessChangedConvention> ForeignKeyRequirednessChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the requiredness of a foreign key is changed.
    /// </summary>
    public virtual List<IForeignKeyDependentRequirednessChangedConvention> ForeignKeyDependentRequirednessChangedConventions { get; }
        = [];

    /// <summary>
    ///     Conventions to run when the ownership of a foreign key is changed.
    /// </summary>
    public virtual List<IForeignKeyOwnershipChangedConvention> ForeignKeyOwnershipChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an annotation is changed on a foreign key.
    /// </summary>
    public virtual List<IForeignKeyAnnotationChangedConvention> ForeignKeyAnnotationChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a navigation is set to <see langword="null" /> on a foreign key.
    /// </summary>
    public virtual List<IForeignKeyNullNavigationSetConvention> ForeignKeyNullNavigationSetConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a navigation property is added.
    /// </summary>
    public virtual List<INavigationAddedConvention> NavigationAddedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an annotation is changed on a navigation property.
    /// </summary>
    public virtual List<INavigationAnnotationChangedConvention> NavigationAnnotationChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a navigation property is removed.
    /// </summary>
    public virtual List<INavigationRemovedConvention> NavigationRemovedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a skip navigation property is added.
    /// </summary>
    public virtual List<ISkipNavigationAddedConvention> SkipNavigationAddedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an annotation is changed on a skip navigation property.
    /// </summary>
    public virtual List<ISkipNavigationAnnotationChangedConvention> SkipNavigationAnnotationChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a skip navigation foreign key is changed.
    /// </summary>
    public virtual List<ISkipNavigationForeignKeyChangedConvention> SkipNavigationForeignKeyChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a skip navigation inverse is changed.
    /// </summary>
    public virtual List<ISkipNavigationInverseChangedConvention> SkipNavigationInverseChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a skip navigation property is removed.
    /// </summary>
    public virtual List<ISkipNavigationRemovedConvention> SkipNavigationRemovedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a trigger property is added.
    /// </summary>
    public virtual List<ITriggerAddedConvention> TriggerAddedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a trigger property is removed.
    /// </summary>
    public virtual List<ITriggerRemovedConvention> TriggerRemovedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a key is added.
    /// </summary>
    public virtual List<IKeyAddedConvention> KeyAddedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a key is removed.
    /// </summary>
    public virtual List<IKeyRemovedConvention> KeyRemovedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an annotation is changed on a key.
    /// </summary>
    public virtual List<IKeyAnnotationChangedConvention> KeyAnnotationChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an index is added.
    /// </summary>
    public virtual List<IIndexAddedConvention> IndexAddedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an index is removed.
    /// </summary>
    public virtual List<IIndexRemovedConvention> IndexRemovedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the uniqueness of an index is changed.
    /// </summary>
    public virtual List<IIndexUniquenessChangedConvention> IndexUniquenessChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the sort order of an index is changed.
    /// </summary>
    public virtual List<IIndexSortOrderChangedConvention> IndexSortOrderChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an annotation is changed on an index.
    /// </summary>
    public virtual List<IIndexAnnotationChangedConvention> IndexAnnotationChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a property is added.
    /// </summary>
    public virtual List<IPropertyAddedConvention> PropertyAddedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the nullability of a property is changed.
    /// </summary>
    public virtual List<IPropertyNullabilityChangedConvention> PropertyNullabilityChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the field of a property is changed.
    /// </summary>
    public virtual List<IPropertyFieldChangedConvention> PropertyFieldChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the field of a property is changed.
    /// </summary>
    public virtual List<IPropertyElementTypeChangedConvention> PropertyElementTypeChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an annotation is changed on a property.
    /// </summary>
    public virtual List<IPropertyAnnotationChangedConvention> PropertyAnnotationChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when a property is removed.
    /// </summary>
    public virtual List<IPropertyRemovedConvention> PropertyRemovedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when the nullability of the element of a collection is changed.
    /// </summary>
    public virtual List<IElementTypeNullabilityChangedConvention> ElementTypeNullabilityChangedConventions { get; } = [];

    /// <summary>
    ///     Conventions to run when an annotation is changed on the element of a collection.
    /// </summary>
    public virtual List<IElementTypeAnnotationChangedConvention> ElementTypeAnnotationChangedConventions { get; } = [];

    /// <summary>
    ///     Replaces an existing convention with a derived convention. Also registers the new convention for any
    ///     convention types not implemented by the existing convention.
    /// </summary>
    /// <typeparam name="TImplementation">The type of the old convention.</typeparam>
    /// <param name="newConvention">The new convention.</param>
    public virtual void Replace<TImplementation>(TImplementation newConvention)
        where TImplementation : IConvention
    {
        var oldConventionType = typeof(TImplementation);
        if (newConvention is IModelInitializedConvention modelInitializedConvention
            && !Replace(ModelInitializedConventions, modelInitializedConvention, oldConventionType))
        {
            ModelInitializedConventions.Add(modelInitializedConvention);
        }

        if (newConvention is IModelFinalizingConvention modelFinalizingConvention
            && !Replace(ModelFinalizingConventions, modelFinalizingConvention, oldConventionType))
        {
            ModelFinalizingConventions.Add(modelFinalizingConvention);
        }

        if (newConvention is IModelFinalizedConvention modelFinalizedConvention
            && !Replace(ModelFinalizedConventions, modelFinalizedConvention, oldConventionType))
        {
            ModelFinalizedConventions.Add(modelFinalizedConvention);
        }

        if (newConvention is IModelAnnotationChangedConvention modelAnnotationChangedConvention
            && !Replace(ModelAnnotationChangedConventions, modelAnnotationChangedConvention, oldConventionType))
        {
            ModelAnnotationChangedConventions.Add(modelAnnotationChangedConvention);
        }

        if (newConvention is ITypeIgnoredConvention typeIgnoredConvention
            && !Replace(TypeIgnoredConventions, typeIgnoredConvention, oldConventionType))
        {
            TypeIgnoredConventions.Add(typeIgnoredConvention);
        }

        if (newConvention is IEntityTypeAddedConvention entityTypeAddedConvention
            && !Replace(EntityTypeAddedConventions, entityTypeAddedConvention, oldConventionType))
        {
            EntityTypeAddedConventions.Add(entityTypeAddedConvention);
        }

        if (newConvention is IEntityTypeRemovedConvention entityTypeRemovedConvention
            && !Replace(EntityTypeRemovedConventions, entityTypeRemovedConvention, oldConventionType))
        {
            EntityTypeRemovedConventions.Add(entityTypeRemovedConvention);
        }

        if (newConvention is IEntityTypeMemberIgnoredConvention entityTypeMemberIgnoredConvention
            && !Replace(EntityTypeMemberIgnoredConventions, entityTypeMemberIgnoredConvention, oldConventionType))
        {
            EntityTypeMemberIgnoredConventions.Add(entityTypeMemberIgnoredConvention);
        }

        if (newConvention is IDiscriminatorPropertySetConvention discriminatorPropertySetConvention
            && !Replace(DiscriminatorPropertySetConventions, discriminatorPropertySetConvention, oldConventionType))
        {
            DiscriminatorPropertySetConventions.Add(discriminatorPropertySetConvention);
        }

        if (newConvention is IEntityTypeBaseTypeChangedConvention entityTypeBaseTypeChangedConvention
            && !Replace(EntityTypeBaseTypeChangedConventions, entityTypeBaseTypeChangedConvention, oldConventionType))
        {
            EntityTypeBaseTypeChangedConventions.Add(entityTypeBaseTypeChangedConvention);
        }

        if (newConvention is IEntityTypePrimaryKeyChangedConvention entityTypePrimaryKeyChangedConvention
            && !Replace(EntityTypePrimaryKeyChangedConventions, entityTypePrimaryKeyChangedConvention, oldConventionType))
        {
            EntityTypePrimaryKeyChangedConventions.Add(entityTypePrimaryKeyChangedConvention);
        }

        if (newConvention is IEntityTypeAnnotationChangedConvention entityTypeAnnotationChangedConvention
            && !Replace(EntityTypeAnnotationChangedConventions, entityTypeAnnotationChangedConvention, oldConventionType))
        {
            EntityTypeAnnotationChangedConventions.Add(entityTypeAnnotationChangedConvention);
        }

        if (newConvention is IComplexPropertyAddedConvention complexPropertyAddedConvention
            && !Replace(ComplexPropertyAddedConventions, complexPropertyAddedConvention, oldConventionType))
        {
            ComplexPropertyAddedConventions.Add(complexPropertyAddedConvention);
        }

        if (newConvention is IComplexPropertyRemovedConvention complexPropertyRemovedConvention
            && !Replace(ComplexPropertyRemovedConventions, complexPropertyRemovedConvention, oldConventionType))
        {
            ComplexPropertyRemovedConventions.Add(complexPropertyRemovedConvention);
        }

        if (newConvention is IComplexTypeAnnotationChangedConvention complexTypeAnnotationChangedConvention
            && !Replace(ComplexTypeAnnotationChangedConventions, complexTypeAnnotationChangedConvention, oldConventionType))
        {
            ComplexTypeAnnotationChangedConventions.Add(complexTypeAnnotationChangedConvention);
        }

        if (newConvention is IComplexTypeMemberIgnoredConvention complexPropertyMemberIgnoredConvention
            && !Replace(ComplexTypeMemberIgnoredConventions, complexPropertyMemberIgnoredConvention, oldConventionType))
        {
            ComplexTypeMemberIgnoredConventions.Add(complexPropertyMemberIgnoredConvention);
        }

        if (newConvention is IComplexPropertyNullabilityChangedConvention complexPropertyNullabilityChangedConvention
            && !Replace(ComplexPropertyNullabilityChangedConventions, complexPropertyNullabilityChangedConvention, oldConventionType))
        {
            ComplexPropertyNullabilityChangedConventions.Add(complexPropertyNullabilityChangedConvention);
        }

        if (newConvention is IComplexPropertyFieldChangedConvention complexPropertyFieldChangedConvention
            && !Replace(ComplexPropertyFieldChangedConventions, complexPropertyFieldChangedConvention, oldConventionType))
        {
            ComplexPropertyFieldChangedConventions.Add(complexPropertyFieldChangedConvention);
        }

        if (newConvention is IComplexPropertyAnnotationChangedConvention complexPropertyAnnotationChangedConvention
            && !Replace(ComplexPropertyAnnotationChangedConventions, complexPropertyAnnotationChangedConvention, oldConventionType))
        {
            ComplexPropertyAnnotationChangedConventions.Add(complexPropertyAnnotationChangedConvention);
        }

        if (newConvention is IForeignKeyAddedConvention foreignKeyAddedConvention
            && !Replace(ForeignKeyAddedConventions, foreignKeyAddedConvention, oldConventionType))
        {
            ForeignKeyAddedConventions.Add(foreignKeyAddedConvention);
        }

        if (newConvention is IForeignKeyRemovedConvention foreignKeyRemovedConvention
            && !Replace(ForeignKeyRemovedConventions, foreignKeyRemovedConvention, oldConventionType))
        {
            ForeignKeyRemovedConventions.Add(foreignKeyRemovedConvention);
        }

        if (newConvention is IForeignKeyPrincipalEndChangedConvention foreignKeyPrincipalEndChangedConvention
            && !Replace(ForeignKeyPrincipalEndChangedConventions, foreignKeyPrincipalEndChangedConvention, oldConventionType))
        {
            ForeignKeyPrincipalEndChangedConventions.Add(foreignKeyPrincipalEndChangedConvention);
        }

        if (newConvention is IForeignKeyPropertiesChangedConvention foreignKeyPropertiesChangedConvention
            && !Replace(ForeignKeyPropertiesChangedConventions, foreignKeyPropertiesChangedConvention, oldConventionType))
        {
            ForeignKeyPropertiesChangedConventions.Add(foreignKeyPropertiesChangedConvention);
        }

        if (newConvention is IForeignKeyUniquenessChangedConvention foreignKeyUniquenessChangedConvention
            && !Replace(ForeignKeyUniquenessChangedConventions, foreignKeyUniquenessChangedConvention, oldConventionType))
        {
            ForeignKeyUniquenessChangedConventions.Add(foreignKeyUniquenessChangedConvention);
        }

        if (newConvention is IForeignKeyRequirednessChangedConvention foreignKeyRequirednessChangedConvention
            && !Replace(ForeignKeyRequirednessChangedConventions, foreignKeyRequirednessChangedConvention, oldConventionType))
        {
            ForeignKeyRequirednessChangedConventions.Add(foreignKeyRequirednessChangedConvention);
        }

        if (newConvention is IForeignKeyDependentRequirednessChangedConvention foreignKeyDependentRequirednessChangedConvention
            && !Replace(
                ForeignKeyDependentRequirednessChangedConventions, foreignKeyDependentRequirednessChangedConvention, oldConventionType))
        {
            ForeignKeyDependentRequirednessChangedConventions.Add(foreignKeyDependentRequirednessChangedConvention);
        }

        if (newConvention is IForeignKeyOwnershipChangedConvention foreignKeyOwnershipChangedConvention
            && !Replace(ForeignKeyOwnershipChangedConventions, foreignKeyOwnershipChangedConvention, oldConventionType))
        {
            ForeignKeyOwnershipChangedConventions.Add(foreignKeyOwnershipChangedConvention);
        }

        if (newConvention is IForeignKeyAnnotationChangedConvention foreignKeyAnnotationChangedConvention
            && !Replace(ForeignKeyAnnotationChangedConventions, foreignKeyAnnotationChangedConvention, oldConventionType))
        {
            ForeignKeyAnnotationChangedConventions.Add(foreignKeyAnnotationChangedConvention);
        }

        if (newConvention is IForeignKeyNullNavigationSetConvention foreignKeyNullNavigationSetConvention
            && !Replace(ForeignKeyNullNavigationSetConventions, foreignKeyNullNavigationSetConvention, oldConventionType))
        {
            ForeignKeyNullNavigationSetConventions.Add(foreignKeyNullNavigationSetConvention);
        }

        if (newConvention is INavigationAddedConvention navigationAddedConvention
            && !Replace(NavigationAddedConventions, navigationAddedConvention, oldConventionType))
        {
            NavigationAddedConventions.Add(navigationAddedConvention);
        }

        if (newConvention is INavigationAnnotationChangedConvention navigationAnnotationChangedConvention
            && !Replace(NavigationAnnotationChangedConventions, navigationAnnotationChangedConvention, oldConventionType))
        {
            NavigationAnnotationChangedConventions.Add(navigationAnnotationChangedConvention);
        }

        if (newConvention is INavigationRemovedConvention navigationRemovedConvention
            && !Replace(NavigationRemovedConventions, navigationRemovedConvention, oldConventionType))
        {
            NavigationRemovedConventions.Add(navigationRemovedConvention);
        }

        if (newConvention is ISkipNavigationAddedConvention skipNavigationAddedConvention
            && !Replace(SkipNavigationAddedConventions, skipNavigationAddedConvention, oldConventionType))
        {
            SkipNavigationAddedConventions.Add(skipNavigationAddedConvention);
        }

        if (newConvention is ISkipNavigationAnnotationChangedConvention skipNavigationAnnotationChangedConvention
            && !Replace(SkipNavigationAnnotationChangedConventions, skipNavigationAnnotationChangedConvention, oldConventionType))
        {
            SkipNavigationAnnotationChangedConventions.Add(skipNavigationAnnotationChangedConvention);
        }

        if (newConvention is ISkipNavigationForeignKeyChangedConvention skipNavigationForeignKeyChangedConvention
            && !Replace(SkipNavigationForeignKeyChangedConventions, skipNavigationForeignKeyChangedConvention, oldConventionType))
        {
            SkipNavigationForeignKeyChangedConventions.Add(skipNavigationForeignKeyChangedConvention);
        }

        if (newConvention is ISkipNavigationInverseChangedConvention skipNavigationInverseChangedConvention
            && !Replace(SkipNavigationInverseChangedConventions, skipNavigationInverseChangedConvention, oldConventionType))
        {
            SkipNavigationInverseChangedConventions.Add(skipNavigationInverseChangedConvention);
        }

        if (newConvention is ISkipNavigationRemovedConvention skipNavigationRemovedConvention
            && !Replace(SkipNavigationRemovedConventions, skipNavigationRemovedConvention, oldConventionType))
        {
            SkipNavigationRemovedConventions.Add(skipNavigationRemovedConvention);
        }

        if (newConvention is IKeyAddedConvention keyAddedConvention
            && !Replace(KeyAddedConventions, keyAddedConvention, oldConventionType))
        {
            KeyAddedConventions.Add(keyAddedConvention);
        }

        if (newConvention is IKeyRemovedConvention keyRemovedConvention
            && !Replace(KeyRemovedConventions, keyRemovedConvention, oldConventionType))
        {
            KeyRemovedConventions.Add(keyRemovedConvention);
        }

        if (newConvention is ITriggerAddedConvention triggerAddedConvention
            && !Replace(TriggerAddedConventions, triggerAddedConvention, oldConventionType))
        {
            TriggerAddedConventions.Add(triggerAddedConvention);
        }

        if (newConvention is ITriggerRemovedConvention triggerRemovedConvention
            && !Replace(TriggerRemovedConventions, triggerRemovedConvention, oldConventionType))
        {
            TriggerRemovedConventions.Add(triggerRemovedConvention);
        }

        if (newConvention is IKeyAnnotationChangedConvention keyAnnotationChangedConvention
            && !Replace(KeyAnnotationChangedConventions, keyAnnotationChangedConvention, oldConventionType))
        {
            KeyAnnotationChangedConventions.Add(keyAnnotationChangedConvention);
        }

        if (newConvention is IIndexAddedConvention indexAddedConvention
            && !Replace(IndexAddedConventions, indexAddedConvention, oldConventionType))
        {
            IndexAddedConventions.Add(indexAddedConvention);
        }

        if (newConvention is IIndexRemovedConvention indexRemovedConvention
            && !Replace(IndexRemovedConventions, indexRemovedConvention, oldConventionType))
        {
            IndexRemovedConventions.Add(indexRemovedConvention);
        }

        if (newConvention is IIndexUniquenessChangedConvention indexUniquenessChangedConvention
            && !Replace(IndexUniquenessChangedConventions, indexUniquenessChangedConvention, oldConventionType))
        {
            IndexUniquenessChangedConventions.Add(indexUniquenessChangedConvention);
        }

        if (newConvention is IIndexSortOrderChangedConvention indexSortOrderChangedConvention
            && !Replace(IndexSortOrderChangedConventions, indexSortOrderChangedConvention, oldConventionType))
        {
            IndexSortOrderChangedConventions.Add(indexSortOrderChangedConvention);
        }

        if (newConvention is IIndexAnnotationChangedConvention indexAnnotationChangedConvention
            && !Replace(IndexAnnotationChangedConventions, indexAnnotationChangedConvention, oldConventionType))
        {
            IndexAnnotationChangedConventions.Add(indexAnnotationChangedConvention);
        }

        if (newConvention is IPropertyAddedConvention propertyAddedConvention
            && !Replace(PropertyAddedConventions, propertyAddedConvention, oldConventionType))
        {
            PropertyAddedConventions.Add(propertyAddedConvention);
        }

        if (newConvention is IPropertyNullabilityChangedConvention propertyNullabilityChangedConvention
            && !Replace(PropertyNullabilityChangedConventions, propertyNullabilityChangedConvention, oldConventionType))
        {
            PropertyNullabilityChangedConventions.Add(propertyNullabilityChangedConvention);
        }

        if (newConvention is IPropertyFieldChangedConvention propertyFieldChangedConvention
            && !Replace(PropertyFieldChangedConventions, propertyFieldChangedConvention, oldConventionType))
        {
            PropertyFieldChangedConventions.Add(propertyFieldChangedConvention);
        }

        if (newConvention is IPropertyAnnotationChangedConvention propertyAnnotationChangedConvention
            && !Replace(PropertyAnnotationChangedConventions, propertyAnnotationChangedConvention, oldConventionType))
        {
            PropertyAnnotationChangedConventions.Add(propertyAnnotationChangedConvention);
        }

        if (newConvention is IPropertyRemovedConvention propertyRemovedConvention
            && !Replace(PropertyRemovedConventions, propertyRemovedConvention, oldConventionType))
        {
            PropertyRemovedConventions.Add(propertyRemovedConvention);
        }

        if (newConvention is IPropertyElementTypeChangedConvention propertyElementTypeChangedConvention
            && !Replace(PropertyElementTypeChangedConventions, propertyElementTypeChangedConvention, oldConventionType))
        {
            PropertyElementTypeChangedConventions.Add(propertyElementTypeChangedConvention);
        }

        if (newConvention is IElementTypeNullabilityChangedConvention elementTypeNullabilityChangedConvention
            && !Replace(ElementTypeNullabilityChangedConventions, elementTypeNullabilityChangedConvention, oldConventionType))
        {
            ElementTypeNullabilityChangedConventions.Add(elementTypeNullabilityChangedConvention);
        }

        if (newConvention is IElementTypeAnnotationChangedConvention elementTypeAnnotationChangedConvention
            && !Replace(ElementTypeAnnotationChangedConventions, elementTypeAnnotationChangedConvention, oldConventionType))
        {
            ElementTypeAnnotationChangedConventions.Add(elementTypeAnnotationChangedConvention);
        }
    }

    /// <summary>
    ///     Replaces an existing convention with a derived convention.
    /// </summary>
    /// <typeparam name="TConvention">The type of convention being replaced.</typeparam>
    /// <typeparam name="TImplementation">The type of the old convention.</typeparam>
    /// <param name="conventionsList">The list of existing convention instances to scan.</param>
    /// <param name="newConvention">The new convention.</param>
    /// <returns><see langword="true" /> if the convention was replaced.</returns>
    public static bool Replace<TConvention, TImplementation>(
        List<TConvention> conventionsList,
        TImplementation newConvention)
        where TImplementation : TConvention
    {
        Check.NotNull(conventionsList, nameof(conventionsList));
        Check.NotNull(newConvention, nameof(newConvention));

        return Replace(conventionsList, newConvention, typeof(TImplementation));
    }

    private static bool Replace<TConvention>(
        List<TConvention> conventionsList,
        TConvention newConvention,
        Type oldConventionType)
    {
        Check.NotNull(conventionsList, nameof(conventionsList));
        Check.NotNull(newConvention, nameof(newConvention));

        var replaced = false;
        for (var i = 0; i < conventionsList.Count; i++)
        {
            if (oldConventionType.IsInstanceOfType(conventionsList[i]!))
            {
                conventionsList.RemoveAt(i);
                conventionsList.Insert(i, newConvention);
                replaced = true;
            }
        }

        return replaced;
    }

    /// <summary>
    ///     Adds a convention to the set.
    /// </summary>
    /// <param name="convention">The convention to add.</param>
    public virtual void Add(IConvention convention)
    {
        if (convention is IModelInitializedConvention modelInitializedConvention)
        {
            ModelInitializedConventions.Add(modelInitializedConvention);
        }

        if (convention is IModelFinalizingConvention modelFinalizingConvention)
        {
            ModelFinalizingConventions.Add(modelFinalizingConvention);
        }

        if (convention is IModelFinalizedConvention modelFinalizedConvention)
        {
            ModelFinalizedConventions.Add(modelFinalizedConvention);
        }

        if (convention is IModelAnnotationChangedConvention modelAnnotationChangedConvention)
        {
            ModelAnnotationChangedConventions.Add(modelAnnotationChangedConvention);
        }

        if (convention is ITypeIgnoredConvention typeIgnoredConvention)
        {
            TypeIgnoredConventions.Add(typeIgnoredConvention);
        }

        if (convention is IEntityTypeAddedConvention entityTypeAddedConvention)
        {
            EntityTypeAddedConventions.Add(entityTypeAddedConvention);
        }

        if (convention is IEntityTypeRemovedConvention entityTypeRemovedConvention)
        {
            EntityTypeRemovedConventions.Add(entityTypeRemovedConvention);
        }

        if (convention is IEntityTypeMemberIgnoredConvention entityTypeMemberIgnoredConvention)
        {
            EntityTypeMemberIgnoredConventions.Add(entityTypeMemberIgnoredConvention);
        }

        if (convention is IDiscriminatorPropertySetConvention discriminatorPropertySetConvention)
        {
            DiscriminatorPropertySetConventions.Add(discriminatorPropertySetConvention);
        }

        if (convention is IEntityTypeBaseTypeChangedConvention entityTypeBaseTypeChangedConvention)
        {
            EntityTypeBaseTypeChangedConventions.Add(entityTypeBaseTypeChangedConvention);
        }

        if (convention is IEntityTypePrimaryKeyChangedConvention entityTypePrimaryKeyChangedConvention)
        {
            EntityTypePrimaryKeyChangedConventions.Add(entityTypePrimaryKeyChangedConvention);
        }

        if (convention is IEntityTypeAnnotationChangedConvention entityTypeAnnotationChangedConvention)
        {
            EntityTypeAnnotationChangedConventions.Add(entityTypeAnnotationChangedConvention);
        }

        if (convention is IComplexPropertyAddedConvention complexPropertyAddedConvention)
        {
            ComplexPropertyAddedConventions.Add(complexPropertyAddedConvention);
        }

        if (convention is IComplexPropertyRemovedConvention complexPropertyRemovedConvention)
        {
            ComplexPropertyRemovedConventions.Add(complexPropertyRemovedConvention);
        }

        if (convention is IComplexTypeMemberIgnoredConvention complexPropertyMemberIgnoredConvention)
        {
            ComplexTypeMemberIgnoredConventions.Add(complexPropertyMemberIgnoredConvention);
        }

        if (convention is IComplexPropertyNullabilityChangedConvention complexPropertyNullabilityChangedConvention)
        {
            ComplexPropertyNullabilityChangedConventions.Add(complexPropertyNullabilityChangedConvention);
        }

        if (convention is IComplexPropertyFieldChangedConvention complexPropertyFieldChangedConvention)
        {
            ComplexPropertyFieldChangedConventions.Add(complexPropertyFieldChangedConvention);
        }

        if (convention is IComplexPropertyAnnotationChangedConvention complexPropertyAnnotationChangedConvention)
        {
            ComplexPropertyAnnotationChangedConventions.Add(complexPropertyAnnotationChangedConvention);
        }

        if (convention is IComplexTypeAnnotationChangedConvention complexTypeAnnotationChangedConvention)
        {
            ComplexTypeAnnotationChangedConventions.Add(complexTypeAnnotationChangedConvention);
        }

        if (convention is IForeignKeyAddedConvention foreignKeyAddedConvention)
        {
            ForeignKeyAddedConventions.Add(foreignKeyAddedConvention);
        }

        if (convention is IForeignKeyRemovedConvention foreignKeyRemovedConvention)
        {
            ForeignKeyRemovedConventions.Add(foreignKeyRemovedConvention);
        }

        if (convention is IForeignKeyPrincipalEndChangedConvention foreignKeyPrincipalEndChangedConvention)
        {
            ForeignKeyPrincipalEndChangedConventions.Add(foreignKeyPrincipalEndChangedConvention);
        }

        if (convention is IForeignKeyPropertiesChangedConvention foreignKeyPropertiesChangedConvention)
        {
            ForeignKeyPropertiesChangedConventions.Add(foreignKeyPropertiesChangedConvention);
        }

        if (convention is IForeignKeyUniquenessChangedConvention foreignKeyUniquenessChangedConvention)
        {
            ForeignKeyUniquenessChangedConventions.Add(foreignKeyUniquenessChangedConvention);
        }

        if (convention is IForeignKeyRequirednessChangedConvention foreignKeyRequirednessChangedConvention)
        {
            ForeignKeyRequirednessChangedConventions.Add(foreignKeyRequirednessChangedConvention);
        }

        if (convention is IForeignKeyDependentRequirednessChangedConvention foreignKeyDependentRequirednessChangedConvention)
        {
            ForeignKeyDependentRequirednessChangedConventions.Add(foreignKeyDependentRequirednessChangedConvention);
        }

        if (convention is IForeignKeyOwnershipChangedConvention foreignKeyOwnershipChangedConvention)
        {
            ForeignKeyOwnershipChangedConventions.Add(foreignKeyOwnershipChangedConvention);
        }

        if (convention is IForeignKeyAnnotationChangedConvention foreignKeyAnnotationChangedConvention)
        {
            ForeignKeyAnnotationChangedConventions.Add(foreignKeyAnnotationChangedConvention);
        }

        if (convention is IForeignKeyNullNavigationSetConvention foreignKeyNullNavigationSetConvention)
        {
            ForeignKeyNullNavigationSetConventions.Add(foreignKeyNullNavigationSetConvention);
        }

        if (convention is INavigationAddedConvention navigationAddedConvention)
        {
            NavigationAddedConventions.Add(navigationAddedConvention);
        }

        if (convention is INavigationAnnotationChangedConvention navigationAnnotationChangedConvention)
        {
            NavigationAnnotationChangedConventions.Add(navigationAnnotationChangedConvention);
        }

        if (convention is INavigationRemovedConvention navigationRemovedConvention)
        {
            NavigationRemovedConventions.Add(navigationRemovedConvention);
        }

        if (convention is ISkipNavigationAddedConvention skipNavigationAddedConvention)
        {
            SkipNavigationAddedConventions.Add(skipNavigationAddedConvention);
        }

        if (convention is ISkipNavigationAnnotationChangedConvention skipNavigationAnnotationChangedConvention)
        {
            SkipNavigationAnnotationChangedConventions.Add(skipNavigationAnnotationChangedConvention);
        }

        if (convention is ISkipNavigationForeignKeyChangedConvention skipNavigationForeignKeyChangedConvention)
        {
            SkipNavigationForeignKeyChangedConventions.Add(skipNavigationForeignKeyChangedConvention);
        }

        if (convention is ISkipNavigationInverseChangedConvention skipNavigationInverseChangedConvention)
        {
            SkipNavigationInverseChangedConventions.Add(skipNavigationInverseChangedConvention);
        }

        if (convention is ISkipNavigationRemovedConvention skipNavigationRemovedConvention)
        {
            SkipNavigationRemovedConventions.Add(skipNavigationRemovedConvention);
        }

        if (convention is ITriggerAddedConvention triggerAddedConvention)
        {
            TriggerAddedConventions.Add(triggerAddedConvention);
        }

        if (convention is ITriggerRemovedConvention triggerRemovedConvention)
        {
            TriggerRemovedConventions.Add(triggerRemovedConvention);
        }

        if (convention is IKeyAddedConvention keyAddedConvention)
        {
            KeyAddedConventions.Add(keyAddedConvention);
        }

        if (convention is IKeyRemovedConvention keyRemovedConvention)
        {
            KeyRemovedConventions.Add(keyRemovedConvention);
        }

        if (convention is IKeyAnnotationChangedConvention keyAnnotationChangedConvention)
        {
            KeyAnnotationChangedConventions.Add(keyAnnotationChangedConvention);
        }

        if (convention is IIndexAddedConvention indexAddedConvention)
        {
            IndexAddedConventions.Add(indexAddedConvention);
        }

        if (convention is IIndexRemovedConvention indexRemovedConvention)
        {
            IndexRemovedConventions.Add(indexRemovedConvention);
        }

        if (convention is IIndexUniquenessChangedConvention indexUniquenessChangedConvention)
        {
            IndexUniquenessChangedConventions.Add(indexUniquenessChangedConvention);
        }

        if (convention is IIndexSortOrderChangedConvention indexSortOrderChangedConvention)
        {
            IndexSortOrderChangedConventions.Add(indexSortOrderChangedConvention);
        }

        if (convention is IIndexAnnotationChangedConvention indexAnnotationChangedConvention)
        {
            IndexAnnotationChangedConventions.Add(indexAnnotationChangedConvention);
        }

        if (convention is IPropertyAddedConvention propertyAddedConvention)
        {
            PropertyAddedConventions.Add(propertyAddedConvention);
        }

        if (convention is IPropertyNullabilityChangedConvention propertyNullabilityChangedConvention)
        {
            PropertyNullabilityChangedConventions.Add(propertyNullabilityChangedConvention);
        }

        if (convention is IPropertyFieldChangedConvention propertyFieldChangedConvention)
        {
            PropertyFieldChangedConventions.Add(propertyFieldChangedConvention);
        }

        if (convention is IPropertyElementTypeChangedConvention propertyElementTypeChangedConvention)
        {
            PropertyElementTypeChangedConventions.Add(propertyElementTypeChangedConvention);
        }

        if (convention is IPropertyAnnotationChangedConvention propertyAnnotationChangedConvention)
        {
            PropertyAnnotationChangedConventions.Add(propertyAnnotationChangedConvention);
        }

        if (convention is IPropertyRemovedConvention propertyRemovedConvention)
        {
            PropertyRemovedConventions.Add(propertyRemovedConvention);
        }

        if (convention is IElementTypeNullabilityChangedConvention elementTypeNullabilityChangedConvention)
        {
            ElementTypeNullabilityChangedConventions.Add(elementTypeNullabilityChangedConvention);
        }

        if (convention is IElementTypeAnnotationChangedConvention elementTypeAnnotationChangedConvention)
        {
            ElementTypeAnnotationChangedConventions.Add(elementTypeAnnotationChangedConvention);
        }
    }

    /// <summary>
    ///     Adds a convention before an existing convention.
    /// </summary>
    /// <typeparam name="TConvention">The type of convention being added.</typeparam>
    /// <param name="conventionsList">The list of existing convention instances to scan.</param>
    /// <param name="newConvention">The new convention.</param>
    /// <param name="existingConventionType">The type of the existing convention.</param>
    /// <returns><see langword="true" /> if the convention was added.</returns>
    public static bool AddBefore<TConvention>(
        List<TConvention> conventionsList,
        TConvention newConvention,
        Type existingConventionType)
    {
        Check.NotNull(conventionsList, nameof(conventionsList));
        Check.NotNull(newConvention, nameof(newConvention));

        for (var i = 0; i < conventionsList.Count; i++)
        {
            if (existingConventionType.IsInstanceOfType(conventionsList[i]))
            {
                conventionsList.Insert(i, newConvention);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Adds a convention after an existing convention.
    /// </summary>
    /// <typeparam name="TConvention">The type of convention being added.</typeparam>
    /// <param name="conventionsList">The list of existing convention instances to scan.</param>
    /// <param name="newConvention">The new convention.</param>
    /// <param name="existingConventionType">The type of the existing convention.</param>
    /// <returns><see langword="true" /> if the convention was added.</returns>
    public static bool AddAfter<TConvention>(
        List<TConvention> conventionsList,
        TConvention newConvention,
        Type existingConventionType)
    {
        Check.NotNull(conventionsList, nameof(conventionsList));
        Check.NotNull(newConvention, nameof(newConvention));

        for (var i = 0; i < conventionsList.Count; i++)
        {
            if (existingConventionType.IsInstanceOfType(conventionsList[i]))
            {
                conventionsList.Insert(i + 1, newConvention);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    ///     Removes the convention of the given type.
    /// </summary>
    /// <param name="conventionType">The convention type to remove.</param>
    public virtual void Remove(Type conventionType)
    {
        if (typeof(IModelInitializedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ModelInitializedConventions, conventionType);
        }

        if (typeof(IModelFinalizingConvention).IsAssignableFrom(conventionType))
        {
            Remove(ModelFinalizingConventions, conventionType);
        }

        if (typeof(IModelFinalizedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ModelFinalizedConventions, conventionType);
        }

        if (typeof(IModelAnnotationChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ModelAnnotationChangedConventions, conventionType);
        }

        if (typeof(ITypeIgnoredConvention).IsAssignableFrom(conventionType))
        {
            Remove(TypeIgnoredConventions, conventionType);
        }

        if (typeof(IEntityTypeAddedConvention).IsAssignableFrom(conventionType))
        {
            Remove(EntityTypeAddedConventions, conventionType);
        }

        if (typeof(IEntityTypeRemovedConvention).IsAssignableFrom(conventionType))
        {
            Remove(EntityTypeRemovedConventions, conventionType);
        }

        if (typeof(IEntityTypeMemberIgnoredConvention).IsAssignableFrom(conventionType))
        {
            Remove(EntityTypeMemberIgnoredConventions, conventionType);
        }

        if (typeof(IDiscriminatorPropertySetConvention).IsAssignableFrom(conventionType))
        {
            Remove(DiscriminatorPropertySetConventions, conventionType);
        }

        if (typeof(IEntityTypeBaseTypeChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(EntityTypeBaseTypeChangedConventions, conventionType);
        }

        if (typeof(IEntityTypePrimaryKeyChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(EntityTypePrimaryKeyChangedConventions, conventionType);
        }

        if (typeof(IEntityTypeAnnotationChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(EntityTypeAnnotationChangedConventions, conventionType);
        }

        if (typeof(IComplexPropertyAddedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ComplexPropertyAddedConventions, conventionType);
        }

        if (typeof(IComplexPropertyRemovedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ComplexPropertyRemovedConventions, conventionType);
        }

        if (typeof(IComplexTypeMemberIgnoredConvention).IsAssignableFrom(conventionType))
        {
            Remove(ComplexTypeMemberIgnoredConventions, conventionType);
        }

        if (typeof(IComplexPropertyNullabilityChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ComplexPropertyNullabilityChangedConventions, conventionType);
        }

        if (typeof(IComplexPropertyFieldChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ComplexPropertyFieldChangedConventions, conventionType);
        }

        if (typeof(IComplexPropertyAnnotationChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ComplexPropertyAnnotationChangedConventions, conventionType);
        }

        if (typeof(IComplexTypeAnnotationChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ComplexTypeAnnotationChangedConventions, conventionType);
        }

        if (typeof(IForeignKeyAddedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ForeignKeyAddedConventions, conventionType);
        }

        if (typeof(IForeignKeyRemovedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ForeignKeyRemovedConventions, conventionType);
        }

        if (typeof(IForeignKeyPrincipalEndChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ForeignKeyPrincipalEndChangedConventions, conventionType);
        }

        if (typeof(IForeignKeyPropertiesChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ForeignKeyPropertiesChangedConventions, conventionType);
        }

        if (typeof(IForeignKeyUniquenessChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ForeignKeyUniquenessChangedConventions, conventionType);
        }

        if (typeof(IForeignKeyRequirednessChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ForeignKeyRequirednessChangedConventions, conventionType);
        }

        if (typeof(IForeignKeyDependentRequirednessChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ForeignKeyDependentRequirednessChangedConventions, conventionType);
        }

        if (typeof(IForeignKeyOwnershipChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ForeignKeyOwnershipChangedConventions, conventionType);
        }

        if (typeof(IForeignKeyAnnotationChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ForeignKeyAnnotationChangedConventions, conventionType);
        }

        if (typeof(IForeignKeyNullNavigationSetConvention).IsAssignableFrom(conventionType))
        {
            Remove(ForeignKeyNullNavigationSetConventions, conventionType);
        }

        if (typeof(INavigationAddedConvention).IsAssignableFrom(conventionType))
        {
            Remove(NavigationAddedConventions, conventionType);
        }

        if (typeof(INavigationAnnotationChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(NavigationAnnotationChangedConventions, conventionType);
        }

        if (typeof(INavigationRemovedConvention).IsAssignableFrom(conventionType))
        {
            Remove(NavigationRemovedConventions, conventionType);
        }

        if (typeof(ISkipNavigationAddedConvention).IsAssignableFrom(conventionType))
        {
            Remove(SkipNavigationAddedConventions, conventionType);
        }

        if (typeof(ISkipNavigationAnnotationChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(SkipNavigationAnnotationChangedConventions, conventionType);
        }

        if (typeof(ISkipNavigationForeignKeyChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(SkipNavigationForeignKeyChangedConventions, conventionType);
        }

        if (typeof(ISkipNavigationInverseChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(SkipNavigationInverseChangedConventions, conventionType);
        }

        if (typeof(ISkipNavigationRemovedConvention).IsAssignableFrom(conventionType))
        {
            Remove(SkipNavigationRemovedConventions, conventionType);
        }

        if (typeof(ITriggerAddedConvention).IsAssignableFrom(conventionType))
        {
            Remove(TriggerAddedConventions, conventionType);
        }

        if (typeof(ITriggerRemovedConvention).IsAssignableFrom(conventionType))
        {
            Remove(TriggerRemovedConventions, conventionType);
        }

        if (typeof(IKeyAddedConvention).IsAssignableFrom(conventionType))
        {
            Remove(KeyAddedConventions, conventionType);
        }

        if (typeof(IKeyRemovedConvention).IsAssignableFrom(conventionType))
        {
            Remove(KeyRemovedConventions, conventionType);
        }

        if (typeof(IKeyAnnotationChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(KeyAnnotationChangedConventions, conventionType);
        }

        if (typeof(IIndexAddedConvention).IsAssignableFrom(conventionType))
        {
            Remove(IndexAddedConventions, conventionType);
        }

        if (typeof(IIndexRemovedConvention).IsAssignableFrom(conventionType))
        {
            Remove(IndexRemovedConventions, conventionType);
        }

        if (typeof(IIndexUniquenessChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(IndexUniquenessChangedConventions, conventionType);
        }

        if (typeof(IIndexSortOrderChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(IndexSortOrderChangedConventions, conventionType);
        }

        if (typeof(IIndexAnnotationChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(IndexAnnotationChangedConventions, conventionType);
        }

        if (typeof(IPropertyAddedConvention).IsAssignableFrom(conventionType))
        {
            Remove(PropertyAddedConventions, conventionType);
        }

        if (typeof(IPropertyNullabilityChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(PropertyNullabilityChangedConventions, conventionType);
        }

        if (typeof(IPropertyFieldChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(PropertyFieldChangedConventions, conventionType);
        }

        if (typeof(IPropertyAnnotationChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(PropertyAnnotationChangedConventions, conventionType);
        }

        if (typeof(IPropertyRemovedConvention).IsAssignableFrom(conventionType))
        {
            Remove(PropertyRemovedConventions, conventionType);
        }

        if (typeof(IPropertyElementTypeChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(PropertyElementTypeChangedConventions, conventionType);
        }

        if (typeof(IElementTypeNullabilityChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ElementTypeNullabilityChangedConventions, conventionType);
        }

        if (typeof(IElementTypeAnnotationChangedConvention).IsAssignableFrom(conventionType))
        {
            Remove(ElementTypeAnnotationChangedConventions, conventionType);
        }
    }

    /// <summary>
    ///     Removes an existing convention.
    /// </summary>
    /// <typeparam name="TConvention">The type of convention being removed.</typeparam>
    /// <param name="conventionsList">The list of existing convention instances to scan.</param>
    /// <param name="existingConventionType">The type of the existing convention.</param>
    /// <returns><see langword="true" /> if the convention was removed.</returns>
    public static bool Remove<TConvention>(
        List<TConvention> conventionsList,
        Type existingConventionType)
    {
        Check.NotNull(conventionsList, nameof(conventionsList));

        var removed = false;
        for (var i = 0; i < conventionsList.Count; i++)
        {
            if (existingConventionType.IsInstanceOfType(conventionsList[i]))
            {
                conventionsList.RemoveAt(i--);
                removed = true;
            }
        }

        return removed;
    }

    /// <summary>
    ///     <para>
    ///         Call this method to build a <see cref="ConventionSet" /> for only core services when using
    ///         the <see cref="ModelBuilder" /> outside of <see cref="DbContext.OnModelCreating" />.
    ///     </para>
    ///     <para>
    ///         Note that it is unusual to use this method. Consider using <see cref="DbContext" /> in the normal way instead.
    ///     </para>
    /// </summary>
    /// <returns>The convention set.</returns>
    public static ConventionSet CreateConventionSet(DbContext context)
        => context.GetService<IConventionSetBuilder>().CreateConventionSet();
}
