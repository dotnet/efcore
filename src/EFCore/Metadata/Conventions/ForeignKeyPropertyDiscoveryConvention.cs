// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that finds foreign key properties for relationships based on their names, ignoring case:
///     * [navigation property name][principal key property name]
///     * [navigation property name]Id
///     * [principal entity name][principal key property name]
///     * [principal entity name]Id
/// </summary>
/// <remarks>
///     <para>
///         If no matching properties were found, the relationship doesn't represent an ownership,
///         the dependent side is not ambiguous and not derived then if the relationship is one-to-one,
///         the primary key properties are used, otherwise the convention tries to match properties with
///         the exact name as principal key properties if they are a proper subset of the dependent PK.
///     </para>
///     <para>
///     </para>
///     <para>
///         If a match was found, but the property types are not compatible with the principal key types no further matches are searched for.
///     </para>
///     <para>
///         If the relationship uses shadow properties created by convention they are recreated to remove suffixes
///         used to make the names unique.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public class ForeignKeyPropertyDiscoveryConvention :
    IEntityTypeMemberIgnoredConvention,
    IEntityTypePrimaryKeyChangedConvention,
    IForeignKeyAddedConvention,
    IForeignKeyPropertiesChangedConvention,
    IForeignKeyPrincipalEndChangedConvention,
    IForeignKeyUniquenessChangedConvention,
    IForeignKeyRequirednessChangedConvention,
    IKeyAddedConvention,
    IKeyRemovedConvention,
    INavigationAddedConvention,
    ISkipNavigationForeignKeyChangedConvention,
    ISkipNavigationInverseChangedConvention,
    IPropertyAddedConvention,
    IPropertyNullabilityChangedConvention,
    IPropertyFieldChangedConvention,
    IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="ForeignKeyPropertyDiscoveryConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public ForeignKeyPropertyDiscoveryConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Called after a foreign key is added to the entity type.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyAdded(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<IConventionForeignKeyBuilder> context)
    {
        var newRelationshipBuilder = ProcessForeignKey(relationshipBuilder, context);

        context.StopProcessingIfChanged(newRelationshipBuilder);
    }

    private IConventionForeignKeyBuilder ProcessForeignKey(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext context)
    {
        var shouldBeRequired = true;
        foreach (var property in relationshipBuilder.Metadata.Properties)
        {
            if (property.IsNullable)
            {
                shouldBeRequired = false;
                relationshipBuilder = relationshipBuilder.IsRequired(false) ?? relationshipBuilder;
                break;
            }
        }

        if (shouldBeRequired)
        {
            relationshipBuilder = relationshipBuilder.IsRequired(true) ?? relationshipBuilder;
        }

        var newRelationshipBuilder = DiscoverProperties(relationshipBuilder, context)!;

        // If new properties were used for this relationship we have to examine the other foreign keys
        // in case they can use the properties used previously.
        var fksToProcess = newRelationshipBuilder.Metadata.DeclaringEntityType.GetForeignKeys()
            .Where(fk => fk != newRelationshipBuilder.Metadata)
            .Concat(
                newRelationshipBuilder.Metadata.DeclaringEntityType.GetDerivedTypes()
                    .SelectMany(et => et.GetDeclaredForeignKeys()))
            .ToList();

        foreach (var fk in fksToProcess)
        {
            if (fk.IsInModel)
            {
                DiscoverProperties(fk.Builder, context);
            }
        }

        return newRelationshipBuilder;
    }

    private IConventionForeignKeyBuilder? DiscoverProperties(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext context)
    {
        var foreignKey = relationshipBuilder.Metadata;
        var foreignKeyProperties = FindCandidateForeignKeyProperties(relationshipBuilder.Metadata, onDependent: true);
        var propertiesConfigurationSource = foreignKey.GetPropertiesConfigurationSource();
        if (!ConfigurationSource.Convention.OverridesStrictly(propertiesConfigurationSource)
            && (propertiesConfigurationSource != ConfigurationSource.Convention
                || (foreignKey.Properties.All(p => !p.IsImplicitlyCreated())
                    && (foreignKeyProperties == null
                        || !foreignKey.Properties.SequenceEqual(foreignKeyProperties)))))
        {
            var batch = context.DelayConventions();
            using var foreignKeyReference = batch.Track(foreignKey);
            foreach (var fkProperty in foreignKey.Properties)
            {
                if (ConfigurationSource.Convention.Overrides(fkProperty.GetTypeConfigurationSource())
                    && (fkProperty.IsShadowProperty() || fkProperty.IsIndexerProperty())
                    && fkProperty.ClrType.IsNullableType() == foreignKey.IsRequired
                    && fkProperty.GetContainingForeignKeys().All(otherFk => otherFk.IsRequired == foreignKey.IsRequired))
                {
                    var newType = fkProperty.ClrType.MakeNullable(!foreignKey.IsRequired);
                    if (fkProperty.ClrType != newType)
                    {
                        fkProperty.DeclaringType.Builder.Property(
                            newType,
                            fkProperty.Name,
                            fkProperty.GetConfigurationSource() == ConfigurationSource.DataAnnotation);
                    }
                }
            }

            batch.Dispose();
            return foreignKeyReference.Object is null || !foreignKeyReference.Object.IsInModel
                ? null
                : foreignKeyReference.Object.Builder;
        }

        var invertible = true;
        if (foreignKey.IsOwnership
            || foreignKey.DeclaringEntityType.IsKeyless
            || (!foreignKey.IsUnique && !ConfigurationSource.Convention.Overrides(foreignKey.GetIsUniqueConfigurationSource()))
            || foreignKey.PrincipalToDependent?.IsCollection == true
            || foreignKey.DeclaringEntityType.FindOwnership() != null
            || (foreignKey.IsBaseLinking()
                && foreignKey.PrincipalEntityType.IsAssignableFrom(foreignKey.DeclaringEntityType)))
        {
            relationshipBuilder = relationshipBuilder.HasEntityTypes(
                foreignKey.PrincipalEntityType, foreignKey.DeclaringEntityType)!;
            invertible = false;
        }
        else if (ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource())
                 && foreignKey.PrincipalEntityType.FindOwnership() != null
                 && foreignKey is { PrincipalToDependent: not null, DependentToPrincipal: null })
        {
            var invertedRelationshipBuilder = relationshipBuilder.HasEntityTypes(
                foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType);
            if (invertedRelationshipBuilder != null)
            {
                return invertedRelationshipBuilder;
            }
        }

        if (foreignKeyProperties == null)
        {
            if (invertible
                && ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource()))
            {
                var candidatePropertiesOnPrincipal = FindCandidateForeignKeyProperties(foreignKey, onDependent: false);
                if (candidatePropertiesOnPrincipal != null)
                {
                    var invertedRelationshipBuilder = relationshipBuilder
                        .HasEntityTypes(foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType);
                    if (invertedRelationshipBuilder is not null
                        && invertedRelationshipBuilder.Metadata is IConventionForeignKey invertedFk
                        && invertedFk.IsSelfReferencing())
                    {
                        invertedRelationshipBuilder = invertedRelationshipBuilder.HasNavigations(
                            invertedFk.PrincipalToDependent?.Name, invertedFk.DependentToPrincipal?.Name);
                    }

                    return invertedRelationshipBuilder ?? (!foreignKey.IsInModel ? null : relationshipBuilder);
                }
            }

            if (foreignKey.DeclaringEntityType.BaseType == null
                && !foreignKey.IsSelfReferencing())
            {
                if (foreignKey.IsUnique)
                {
                    // Try to use PK properties if principal end is not ambiguous
                    if (!foreignKey.IsOwnership
                        && (!ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource())
                            || foreignKey.DeclaringEntityType.IsInOwnershipPath(foreignKey.PrincipalEntityType)))
                    {
                        foreignKeyProperties = GetCompatiblePrimaryKeyProperties(
                            foreignKey.DeclaringEntityType,
                            foreignKey.PrincipalEntityType,
                            foreignKey.PrincipalKey.Properties);
                    }
                    // Try to use PK properties on owner if configured explicitly or on CLR properties
                    else if (foreignKey.IsOwnership
                             && (!ConfigurationSource.Convention.Overrides(
                                     foreignKey.DeclaringEntityType.GetPrimaryKeyConfigurationSource())
                                 || (foreignKey.DeclaringEntityType.FindPrimaryKey()?.Properties.All(p => !p.IsShadowProperty()) ?? false)))
                    {
                        foreignKeyProperties = GetCompatiblePrimaryKeyProperties(
                            foreignKey.DeclaringEntityType,
                            foreignKey.PrincipalEntityType,
                            foreignKey.PrincipalKey.Properties);
                    }
                    else if (invertible)
                    {
                        foreignKeyProperties = FindCandidateForeignKeyProperties(foreignKey, onDependent: true, matchPk: true);
                        var candidatePropertiesOnPrincipal =
                            FindCandidateForeignKeyProperties(foreignKey, onDependent: false, matchPk: true);
                        if (candidatePropertiesOnPrincipal != null)
                        {
                            if (foreignKeyProperties == null)
                            {
                                using var batch = context.DelayConventions();
                                var invertedRelationshipBuilder = relationshipBuilder
                                    .HasEntityTypes(foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType)!;
                                return batch.Run(
                                        invertedRelationshipBuilder.HasForeignKey(candidatePropertiesOnPrincipal)!.Metadata)
                                    ?.Builder;
                            }

                            foreignKeyProperties = null;
                            ((ForeignKey)relationshipBuilder.Metadata).SetPrincipalEndConfigurationSource(null);
                        }
                    }
                }
                else
                {
                    // Try match properties with the exact name as principal key if they are a proper subset of the dependent PK
                    var dependentPk = foreignKey.DeclaringEntityType.FindPrimaryKey();
                    if (dependentPk != null
                        && dependentPk.Properties.Count > foreignKey.PrincipalKey.Properties.Count
                        && TryFindMatchingProperties(foreignKey, "", onDependent: true, matchPk: false, out foreignKeyProperties)
                        && foreignKeyProperties != null
                        && foreignKeyProperties.Any(
                            p => !dependentPk.Properties.Contains(p)
                                || p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase)))
                    {
                        foreignKeyProperties = null;
                    }
                }
            }

            if (foreignKeyProperties == null
                && invertible
                && ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource()))
            {
                ((ForeignKey)relationshipBuilder.Metadata).SetPrincipalEndConfigurationSource(null);
            }
        }
        else if (invertible
                 && ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource()))
        {
            var candidatePropertiesOnPrincipal = FindCandidateForeignKeyProperties(foreignKey, onDependent: false);
            if (candidatePropertiesOnPrincipal != null)
            {
                // Principal end is ambiguous
                foreignKeyProperties = null;
                ((ForeignKey)relationshipBuilder.Metadata).SetPrincipalEndConfigurationSource(null);
            }
        }

        if (foreignKeyProperties == null)
        {
            return ((ForeignKey)foreignKey).Builder.ReuniquifyImplicitProperties(false);
        }

        var conflictingFKCount = foreignKey.DeclaringEntityType.FindForeignKeys(foreignKeyProperties)
                .Concat(
                    foreignKey.DeclaringEntityType.GetDerivedTypes()
                        .SelectMany(et => et.FindDeclaredForeignKeys(foreignKeyProperties)))
                .Count()
            - 1;
        if (foreignKey.Properties.SequenceEqual(foreignKeyProperties))
        {
            return conflictingFKCount > 0 && !foreignKey.IsOwnership
                ? ((ForeignKey)foreignKey).Builder.ReuniquifyImplicitProperties(true)
                : relationshipBuilder;
        }

        if (conflictingFKCount >= 0)
        {
            return ((ForeignKey)foreignKey).Builder.ReuniquifyImplicitProperties(false);
        }

        var newRelationshipBuilder = relationshipBuilder.HasForeignKey(foreignKeyProperties);
        if (newRelationshipBuilder != null)
        {
            return newRelationshipBuilder;
        }

        return relationshipBuilder.Metadata.IsInModel ? relationshipBuilder : null;
    }

    private IReadOnlyList<IConventionProperty>? FindCandidateForeignKeyProperties(
        IConventionForeignKey foreignKey,
        bool onDependent,
        bool matchPk = false)
    {
        IReadOnlyList<IConventionProperty>? match;
        if (onDependent)
        {
            foreach (var skipNavigation in foreignKey.GetReferencingSkipNavigations())
            {
                if (skipNavigation.Inverse == null)
                {
                    continue;
                }

                if (TryFindMatchingProperties(foreignKey, skipNavigation.Inverse.Name, onDependent, matchPk: true, out match))
                {
                    return match;
                }
            }
        }

        var navigation = onDependent
            ? foreignKey.DependentToPrincipal
            : foreignKey.PrincipalToDependent;
        if (navigation != null)
        {
            if (TryFindMatchingProperties(foreignKey, navigation.Name, onDependent, matchPk, out match))
            {
                return match;
            }
        }

        var entityTypeToReference = onDependent
            ? foreignKey.PrincipalEntityType
            : foreignKey.DeclaringEntityType;

        TryFindMatchingProperties(foreignKey, entityTypeToReference.ShortName(), onDependent, matchPk, out match);
        return match;
    }

    private static IReadOnlyList<IConventionProperty>? GetCompatiblePrimaryKeyProperties(
        IConventionEntityType dependentEntityType,
        IConventionEntityType principalEntityType,
        IReadOnlyList<IConventionProperty> propertiesToReference)
    {
        var dependentPkProperties = dependentEntityType.FindPrimaryKey()?.Properties;
        return dependentPkProperties != null
            && ForeignKey.AreCompatible(
                propertiesToReference,
                dependentPkProperties,
                principalEntityType,
                dependentEntityType,
                shouldThrow: false)
                ? dependentPkProperties
                : null;
    }

    private bool TryFindMatchingProperties(
        IConventionForeignKey foreignKey,
        string baseName,
        bool onDependent,
        bool matchPk,
        out IReadOnlyList<IConventionProperty>? match)
    {
        match = null;
        var dependentEntityType = onDependent
            ? foreignKey.DeclaringEntityType
            : foreignKey.PrincipalEntityType;
        var principalEntityType = onDependent
            ? foreignKey.PrincipalEntityType
            : foreignKey.DeclaringEntityType;
        var propertiesToReference = onDependent
            ? foreignKey.PrincipalKey.Properties
            : foreignKey.DeclaringEntityType.FindPrimaryKey()?.Properties;

        if (propertiesToReference == null)
        {
            return false;
        }

        var foreignKeyProperties = new IConventionProperty[propertiesToReference.Count];
        var matchFound = true;
        for (var i = 0; i < propertiesToReference.Count; i++)
        {
            var referencedProperty = propertiesToReference[i];
            var property = TryGetProperty(
                dependentEntityType,
                baseName,
                referencedProperty.Name,
                matchImplicitProperties: propertiesToReference.Count != 1);

            if (property == null)
            {
                matchFound = false;
                continue;
            }

            foreignKeyProperties[i] = property;
        }

        if (matchFound
            && foreignKeyProperties.Length != 1
            && foreignKeyProperties.All(
                p => p.IsImplicitlyCreated()
                    && ConfigurationSource.Convention.Overrides(p.GetConfigurationSource())))
        {
            return false;
        }

        if (!matchFound
            && propertiesToReference.Count == 1
            && baseName.Length > 0)
        {
            var property = TryGetProperty(
                dependentEntityType,
                baseName, "Id", matchImplicitProperties: false);

            if (property != null)
            {
                foreignKeyProperties[0] = property;
                matchFound = true;
            }
        }

        if (!matchFound)
        {
            return false;
        }

        if (!ForeignKey.AreCompatible(
                propertiesToReference,
                foreignKeyProperties,
                principalEntityType,
                dependentEntityType,
                shouldThrow: false))
        {
            if (propertiesToReference.All(
                    p => !p.IsImplicitlyCreated()
                        || p.GetConfigurationSource().Overrides(ConfigurationSource.DataAnnotation)))
            {
                var dependentNavigationSpec = onDependent
                    ? foreignKey.DependentToPrincipal?.Name
                    : foreignKey.PrincipalToDependent?.Name;
                dependentNavigationSpec = dependentEntityType.DisplayName()
                    + (string.IsNullOrEmpty(dependentNavigationSpec)
                        ? string.Empty
                        : "." + dependentNavigationSpec);

                var principalNavigationSpec = onDependent
                    ? foreignKey.PrincipalToDependent?.Name
                    : foreignKey.DependentToPrincipal?.Name;
                principalNavigationSpec = principalEntityType.DisplayName()
                    + (string.IsNullOrEmpty(principalNavigationSpec)
                        ? string.Empty
                        : "." + principalNavigationSpec);

                Dependencies.Logger.IncompatibleMatchingForeignKeyProperties(
                    dependentNavigationSpec, principalNavigationSpec,
                    foreignKeyProperties, propertiesToReference);
            }

            // Stop searching if match found, but is incompatible
            return true;
        }

        foreach (var key in dependentEntityType.GetKeys())
        {
            var isKeyContainedInForeignKey = true;
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < key.Properties.Count; i++)
            {
                if (!foreignKeyProperties.Contains(key.Properties[i]))
                {
                    isKeyContainedInForeignKey = false;
                    break;
                }
            }

            if (isKeyContainedInForeignKey
                && (!foreignKey.IsUnique
                    || (key.IsPrimaryKey()
                        && !matchPk)))
            {
                // Stop searching if match found, but is incompatible
                return true;
            }
        }

        match = foreignKeyProperties;
        return true;
    }

    private static IConventionProperty? TryGetProperty(
        IConventionEntityType entityType,
        string prefix,
        string suffix,
        bool matchImplicitProperties)
    {
        foreach (var property in entityType.GetProperties())
        {
            if ((!property.IsImplicitlyCreated()
                    || matchImplicitProperties
                    || !ConfigurationSource.Convention.Overrides(property.GetConfigurationSource()))
                && property.Name.Length == prefix.Length + suffix.Length
                && property.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && property.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                return property;
            }
        }

        return null;
    }

    /// <inheritdoc />
    public virtual void ProcessNavigationAdded(
        IConventionNavigationBuilder navigationBuilder,
        IConventionContext<IConventionNavigationBuilder> context)
    {
        var navigation = navigationBuilder.Metadata;
        var newRelationshipBuilder = DiscoverProperties(navigation.ForeignKey.Builder, context);
        if (newRelationshipBuilder != null)
        {
            context.StopProcessingIfChanged(newRelationshipBuilder.Metadata.GetNavigation(navigation.IsOnDependent)?.Builder);
        }
    }

    /// <summary>
    ///     Called after a property is added to the entity type.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<IConventionPropertyBuilder> context)
    {
        Process(propertyBuilder, context);
        if (!propertyBuilder.Metadata.IsInModel)
        {
            context.StopProcessing();
        }
    }

    private void Process(IConventionPropertyBuilder propertyBuilder, IConventionContext context)
    {
        var property = propertyBuilder.Metadata;
        if ((property.IsImplicitlyCreated()
                && ConfigurationSource.Convention.Overrides(property.GetConfigurationSource()))
            || propertyBuilder.Metadata.DeclaringType is not IConventionEntityType entityType)
        {
            return;
        }

        Process(entityType, context);
    }

    /// <summary>
    ///     Called after an entity type member is ignored.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="name">The name of the ignored member.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeMemberIgnored(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionContext<string> context)
        => Process(entityTypeBuilder.Metadata, context);

    private void Process(IConventionEntityType entityType, IConventionContext context)
    {
        foreach (var foreignKey in entityType.GetDeclaredForeignKeys().Concat(entityType.GetDerivedForeignKeys()).ToList())
        {
            if (foreignKey.IsInModel)
            {
                DiscoverProperties(foreignKey.Builder, context);
            }
        }

        foreach (var foreignKey in entityType.GetReferencingForeignKeys().ToList())
        {
            if (foreignKey.IsInModel
                && ConfigurationSource.Convention.Overrides(foreignKey.GetPrincipalEndConfigurationSource()))
            {
                DiscoverProperties(foreignKey.Builder, context);
            }
        }
    }

    /// <summary>
    ///     Called after the backing field for a property is changed.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="newFieldInfo">The new field.</param>
    /// <param name="oldFieldInfo">The old field.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessPropertyFieldChanged(
        IConventionPropertyBuilder propertyBuilder,
        FieldInfo? newFieldInfo,
        FieldInfo? oldFieldInfo,
        IConventionContext<FieldInfo> context)
        => Process(propertyBuilder, context);

    /// <summary>
    ///     Called after the nullability for a property is changed.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessPropertyNullabilityChanged(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<bool?> context)
    {
        var nullable = propertyBuilder.Metadata.IsNullable;
        foreach (var containingForeignKey in propertyBuilder.Metadata.GetContainingForeignKeys())
        {
            if (containingForeignKey.IsRequired != nullable
                || (!nullable
                    && containingForeignKey.Properties.Any(p => p.IsNullable)))
            {
                continue;
            }

            containingForeignKey.Builder.IsRequired(!nullable);
        }
    }

    /// <summary>
    ///     Called after the uniqueness for a foreign key is changed.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyUniquenessChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<bool?> context)
    {
        var newRelationshipBuilder = DiscoverProperties(relationshipBuilder, context)!;
        context.StopProcessingIfChanged(newRelationshipBuilder.Metadata.IsUnique);
    }

    /// <summary>
    ///     Called after the requiredness for a foreign key is changed.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyRequirednessChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<bool?> context)
    {
        var isRequired = relationshipBuilder.Metadata.IsRequired;
        using var batch = context.DelayConventions();
        foreach (var property in relationshipBuilder.Metadata.Properties.Where(p => p.ClrType.IsNullableType()))
        {
            var requiredSet = property.Builder.IsRequired(isRequired) != null;
            if (requiredSet
                && isRequired != true)
            {
                break;
            }
        }

        var newForeignKey = batch.Run(DiscoverProperties(relationshipBuilder, context)!.Metadata);
        if (newForeignKey != relationshipBuilder.Metadata
            || newForeignKey.IsRequired != isRequired)
        {
            context.StopProcessing();
        }
    }

    /// <summary>
    ///     Called after the foreign key properties or principal key are changed.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="oldDependentProperties">The old foreign key properties.</param>
    /// <param name="oldPrincipalKey">The old principal key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyPropertiesChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IReadOnlyList<IConventionProperty> oldDependentProperties,
        IConventionKey oldPrincipalKey,
        IConventionContext<IReadOnlyList<IConventionProperty>> context)
    {
        if (!relationshipBuilder.Metadata.IsInModel
            || relationshipBuilder.Metadata.Properties == oldDependentProperties)
        {
            return;
        }

        ProcessForeignKey(relationshipBuilder, context);

        context.StopProcessingIfChanged(relationshipBuilder.Metadata.Properties);
    }

    /// <summary>
    ///     Called after the principal end of a foreign key is changed.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyPrincipalEndChanged(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<IConventionForeignKeyBuilder> context)
        => ProcessForeignKeyAdded(relationshipBuilder, context);

    /// <summary>
    ///     Called after a key is added to the entity type.
    /// </summary>
    /// <param name="keyBuilder">The builder for the key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessKeyAdded(IConventionKeyBuilder keyBuilder, IConventionContext<IConventionKeyBuilder> context)
    {
        var key = keyBuilder.Metadata;
        foreach (var foreignKey in key.DeclaringEntityType.GetDerivedTypesInclusive()
                     .SelectMany(t => t.GetDeclaredForeignKeys()).ToList())
        {
            if (key.Properties.All(p => foreignKey.Properties.Contains(p))
                && (!foreignKey.IsUnique || foreignKey.DeclaringEntityType.BaseType != null))
            {
                foreignKey.Builder.HasForeignKey((IReadOnlyList<IConventionProperty>?)null);
            }
        }
    }

    /// <inheritdoc />
    public virtual void ProcessKeyRemoved(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionKey key,
        IConventionContext<IConventionKey> context)
    {
        if (!entityTypeBuilder.Metadata.IsInModel)
        {
            return;
        }

        var foreignKeys = key.DeclaringEntityType.GetDerivedTypesInclusive()
            .SelectMany(t => t.GetDeclaredForeignKeys()).ToList();
        foreach (var foreignKey in foreignKeys)
        {
            if (foreignKey is { IsUnique: true, DeclaringEntityType.BaseType: null }
                || !foreignKey.IsInModel)
            {
                continue;
            }

            DiscoverProperties(foreignKey.Builder, context);
        }
    }

    /// <summary>
    ///     Called after the primary key for an entity type is changed.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="newPrimaryKey">The new primary key.</param>
    /// <param name="previousPrimaryKey">The old primary key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypePrimaryKeyChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionKey? newPrimaryKey,
        IConventionKey? previousPrimaryKey,
        IConventionContext<IConventionKey> context)
    {
        if (newPrimaryKey is { IsInModel: false })
        {
            return;
        }

        var initialPrimaryKey = entityTypeBuilder.Metadata.FindPrimaryKey();

        var foreignKeys = entityTypeBuilder.Metadata.GetDerivedTypesInclusive()
            .SelectMany(t => t.GetDeclaredForeignKeys()).ToList();
        foreach (var foreignKey in foreignKeys)
        {
            if (!foreignKey.IsUnique
                || !foreignKey.IsInModel)
            {
                continue;
            }

            DiscoverProperties(foreignKey.Builder, context);
        }

        var referencingForeignKeys = entityTypeBuilder.Metadata.GetDerivedTypesInclusive()
            .SelectMany(t => t.GetDeclaredReferencingForeignKeys()).ToList();
        foreach (var referencingForeignKey in referencingForeignKeys)
        {
            if (referencingForeignKey.IsSelfReferencing()
                || !referencingForeignKey.IsInModel)
            {
                continue;
            }

            DiscoverProperties(referencingForeignKey.Builder, context);
        }

        if (entityTypeBuilder.Metadata.FindPrimaryKey() != initialPrimaryKey)
        {
            context.StopProcessing();
        }
    }

    /// <inheritdoc />
    public virtual void ProcessSkipNavigationForeignKeyChanged(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        IConventionForeignKey? foreignKey,
        IConventionForeignKey? oldForeignKey,
        IConventionContext<IConventionForeignKey> context)
    {
        if (foreignKey?.IsInModel == true
            && foreignKey.GetPropertiesConfigurationSource() == null
            && skipNavigationBuilder.Metadata.Inverse?.IsInModel == true)
        {
            DiscoverProperties(foreignKey.Builder, context);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessSkipNavigationInverseChanged(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        IConventionSkipNavigation? inverse,
        IConventionSkipNavigation? oldInverse,
        IConventionContext<IConventionSkipNavigation> context)
    {
        var foreignKey = skipNavigationBuilder.Metadata.ForeignKey;
        if (foreignKey != null
            && foreignKey.GetPropertiesConfigurationSource() == null
            && inverse?.IsInModel == true)
        {
            DiscoverProperties(foreignKey.Builder, context);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            var conflictingFkFound = false;
            foreach (var foreignKey in entityType.GetDeclaredForeignKeys().Where(fk => fk.GetPropertiesConfigurationSource() == null))
            {
                var foreignKeyProperties = FindCandidateForeignKeyProperties(foreignKey, onDependent: true);
                if (foreignKeyProperties != null)
                {
                    var conflictingForeignKey = foreignKey.DeclaringEntityType.FindForeignKeys(foreignKeyProperties).Concat(
                            foreignKey.DeclaringEntityType.GetDerivedTypes()
                                .SelectMany(et => et.FindDeclaredForeignKeys(foreignKeyProperties)))
                        .FirstOrDefault(
                            fk => fk != foreignKey
                                && ConfigurationSource.Convention.Overrides(fk.GetPropertiesConfigurationSource()));
                    if (conflictingForeignKey != null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.AmbiguousForeignKeyPropertyCandidates(
                                conflictingForeignKey.DeclaringEntityType.DisplayName()
                                + (conflictingForeignKey.DependentToPrincipal == null
                                    ? ""
                                    : "." + conflictingForeignKey.DependentToPrincipal.Name),
                                conflictingForeignKey.PrincipalEntityType.DisplayName()
                                + (conflictingForeignKey.PrincipalToDependent == null
                                    ? ""
                                    : "." + conflictingForeignKey.PrincipalToDependent.Name),
                                foreignKey.DeclaringEntityType.DisplayName()
                                + (foreignKey.DependentToPrincipal == null ? "" : "." + foreignKey.DependentToPrincipal.Name),
                                foreignKey.PrincipalEntityType.DisplayName()
                                + (foreignKey.PrincipalToDependent == null ? "" : "." + foreignKey.PrincipalToDependent.Name),
                                foreignKeyProperties.Format()));
                    }
                }

                if (conflictingFkFound)
                {
                    continue;
                }

                if (HasUniquifiedProperties(foreignKey))
                {
                    var conflictingFk = entityType.GetDeclaredForeignKeys().FirstOrDefault(
                        otherForeignKey =>
                            otherForeignKey != foreignKey
                            && otherForeignKey.PrincipalEntityType == foreignKey.PrincipalEntityType
                            && otherForeignKey.GetPropertiesConfigurationSource() == null);
                    if (conflictingFk != null)
                    {
                        conflictingFkFound = true;
                        Dependencies.Logger.ConflictingShadowForeignKeysWarning(conflictingFk);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Gets the string that should be used as part of the shadow properties created for the given foreign key.
    /// </summary>
    /// <param name="foreignKey">The foreign key.</param>
    /// <returns>The string that should be used as part of the shadow properties created for the given foreign key.</returns>
    public static string GetPropertyBaseName(IReadOnlyForeignKey foreignKey)
        => foreignKey.DependentToPrincipal?.Name
            ?? foreignKey.GetReferencingSkipNavigations().FirstOrDefault()?.Inverse?.Name
            ?? foreignKey.PrincipalEntityType.ShortName();

    private static bool HasUniquifiedProperties(IConventionForeignKey foreignKey)
    {
        if (foreignKey.GetPropertiesConfigurationSource() != null)
        {
            return false;
        }

        var fkBaseName = GetPropertyBaseName(foreignKey);
        for (var i = 0; i < foreignKey.Properties.Count; i++)
        {
            var property = foreignKey.Properties[i];
            if (!ConfigurationSource.Convention.Overrides(property.GetConfigurationSource())
                || !property.IsImplicitlyCreated())
            {
                return false;
            }

            var fkPropertyName = property.Name;
            var pkPropertyName = foreignKey.PrincipalKey.Properties[i].Name;
            if (fkPropertyName.Length != fkBaseName.Length + pkPropertyName.Length
                || !fkPropertyName.StartsWith(fkBaseName, StringComparison.Ordinal)
                || !fkPropertyName.EndsWith(pkPropertyName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
