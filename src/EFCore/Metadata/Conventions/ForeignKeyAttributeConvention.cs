// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the foreign key properties associated with a navigation property
///     based on the <see cref="ForeignKeyAttribute" /> specified on the properties or the navigation properties.
/// </summary>
/// <remarks>
///     <para>
///         For one-to-one relationships the attribute has to be specified on the navigation property pointing to the principal.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
///     </para>
/// </remarks>
public class ForeignKeyAttributeConvention :
    IEntityTypeAddedConvention,
    IForeignKeyAddedConvention,
    INavigationAddedConvention,
    ISkipNavigationForeignKeyChangedConvention,
    IPropertyAddedConvention,
    IComplexPropertyAddedConvention,
    IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="ForeignKeyAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public ForeignKeyAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionContext<IConventionEntityTypeBuilder> context)
    {
        // Configure self-ref navigations that would be ambiguous without the attributes
        var entityType = entityTypeBuilder.Metadata;
        var foreignKeyNavigations = new List<PropertyInfo>();
        var unconfiguredNavigations = new List<PropertyInfo>();
        var inverses = new List<string>();
        foreach (var candidatePair in Dependencies.MemberClassifier.GetNavigationCandidates(entityType, useAttributes: true))
        {
            var (targetType, shouldBeOwned) = candidatePair.Value;
            if (targetType != entityType.ClrType)
            {
                continue;
            }

            if (shouldBeOwned == true)
            {
                return;
            }

            var navigation = candidatePair.Key;
            var inverseProperty = GetAttribute<InversePropertyAttribute>(navigation)?.Property;
            if (inverseProperty != null)
            {
                inverses.Add(inverseProperty);
                continue;
            }

            if (!Attribute.IsDefined(navigation, typeof(ForeignKeyAttribute), inherit: true))
            {
                if (FindForeignKeyAttributeOnProperty(entityType, navigation) == null)
                {
                    unconfiguredNavigations.Add(navigation);
                    continue;
                }
            }

            foreignKeyNavigations.Add(navigation);
        }

        foreach (var inverse in inverses)
        {
            unconfiguredNavigations.RemoveAll(n => string.Equals(n.GetSimpleMemberName(), inverse, StringComparison.Ordinal));
            foreignKeyNavigations.RemoveAll(n => string.Equals(n.GetSimpleMemberName(), inverse, StringComparison.Ordinal));
        }

        if (unconfiguredNavigations.Count == 1)
        {
            if (foreignKeyNavigations.Count == 1)
            {
                entityTypeBuilder.HasRelationship(
                    entityType, foreignKeyNavigations[0], unconfiguredNavigations[0], fromDataAnnotation: true);
            }

            return;
        }

        if (unconfiguredNavigations.Count > 1)
        {
            return;
        }

        foreach (var navigation in foreignKeyNavigations)
        {
            entityTypeBuilder.HasRelationship(
                entityType,
                navigation,
                setTargetAsPrincipal: navigation.GetMemberType().IsAssignableFrom(entityType.ClrType),
                fromDataAnnotation: true);
        }
    }

    /// <summary>
    ///     Called after a foreign key is added to the entity type.
    /// </summary>
    /// <param name="relationshipBuilder">The builder for the foreign key.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessForeignKeyAdded(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext<IConventionForeignKeyBuilder> context)
    {
        Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));

        var newRelationshipBuilder = UpdateRelationshipBuilder(relationshipBuilder, context);
        if (newRelationshipBuilder != null)
        {
            context.StopProcessingIfChanged(newRelationshipBuilder);
        }
    }

    /// <summary>
    ///     Called after a navigation is added to the entity type.
    /// </summary>
    /// <param name="navigationBuilder">The builder for the navigation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessNavigationAdded(
        IConventionNavigationBuilder navigationBuilder,
        IConventionContext<IConventionNavigationBuilder> context)
    {
        Check.NotNull(navigationBuilder, nameof(navigationBuilder));

        var onDependent = navigationBuilder.Metadata.IsOnDependent;
        var newRelationshipBuilder = UpdateRelationshipBuilder(navigationBuilder.Metadata.ForeignKey.Builder, context);
        if (newRelationshipBuilder != null)
        {
            var newNavigationBuilder = onDependent
                ? newRelationshipBuilder.Metadata.DependentToPrincipal!.Builder
                : newRelationshipBuilder.Metadata.PrincipalToDependent!.Builder;
            context.StopProcessingIfChanged(newNavigationBuilder);
        }
    }

    private IConventionForeignKeyBuilder? UpdateRelationshipBuilder(
        IConventionForeignKeyBuilder relationshipBuilder,
        IConventionContext context)
    {
        var foreignKey = relationshipBuilder.Metadata;

        var fkPropertyOnPrincipal
            = FindForeignKeyAttributeOnProperty(
                foreignKey.PrincipalEntityType, foreignKey.PrincipalToDependent?.GetIdentifyingMemberInfo());

        var fkPropertyOnDependent
            = FindForeignKeyAttributeOnProperty(
                foreignKey.DeclaringEntityType, foreignKey.DependentToPrincipal?.GetIdentifyingMemberInfo());

        if (fkPropertyOnDependent != null
            && fkPropertyOnPrincipal != null)
        {
            Dependencies.Logger.ForeignKeyAttributesOnBothPropertiesWarning(
                foreignKey.PrincipalToDependent!,
                foreignKey.DependentToPrincipal!,
                fkPropertyOnPrincipal,
                fkPropertyOnDependent);

            var newBuilder = SplitNavigationsToSeparateRelationships(relationshipBuilder);
            if (newBuilder is null)
            {
                context.StopProcessing();
                return null;
            }

            relationshipBuilder = newBuilder;
            fkPropertyOnPrincipal = null;
        }

        var fkPropertiesOnPrincipalToDependent
            = FindCandidateDependentPropertiesThroughNavigation(relationshipBuilder, pointsToPrincipal: false);

        var fkPropertiesOnDependentToPrincipal
            = FindCandidateDependentPropertiesThroughNavigation(relationshipBuilder, pointsToPrincipal: true);

        if (fkPropertiesOnDependentToPrincipal != null
            && fkPropertiesOnPrincipalToDependent != null)
        {
            Dependencies.Logger.ForeignKeyAttributesOnBothNavigationsWarning(
                relationshipBuilder.Metadata.DependentToPrincipal!, relationshipBuilder.Metadata.PrincipalToDependent!);

            var newBuilder = SplitNavigationsToSeparateRelationships(relationshipBuilder);
            if (newBuilder is null)
            {
                context.StopProcessing();
                return null;
            }

            relationshipBuilder = newBuilder;
            fkPropertiesOnPrincipalToDependent = null;
        }

        var fkPropertiesOnNavigation = fkPropertiesOnDependentToPrincipal ?? fkPropertiesOnPrincipalToDependent;
        var upgradePrincipalToDependentNavigationSource = fkPropertiesOnPrincipalToDependent != null;
        var upgradeDependentToPrincipalNavigationSource = fkPropertiesOnDependentToPrincipal != null;
        var shouldInvert = false;
        IReadOnlyList<string> fkPropertiesToSet;

        if (fkPropertiesOnNavigation == null
            || fkPropertiesOnNavigation.Count == 0)
        {
            if (fkPropertyOnDependent == null
                && fkPropertyOnPrincipal == null)
            {
                return null;
            }

            if (fkPropertyOnDependent != null)
            {
                fkPropertiesToSet = new List<string> { fkPropertyOnDependent.GetSimpleMemberName() };
                upgradeDependentToPrincipalNavigationSource = true;
            }
            else
            {
                if (foreignKey.PrincipalToDependent!.IsCollection)
                {
                    context.StopProcessing();
                    return null;
                }

                shouldInvert = true;
                fkPropertiesToSet = new List<string> { fkPropertyOnPrincipal!.GetSimpleMemberName() };
                upgradePrincipalToDependentNavigationSource = true;
            }
        }
        else
        {
            fkPropertiesToSet = fkPropertiesOnNavigation;

            if (fkPropertyOnDependent == null
                && fkPropertyOnPrincipal == null)
            {
                if (fkPropertiesOnPrincipalToDependent != null
                    && foreignKey.IsUnique)
                {
                    shouldInvert = true;
                }
            }
            else
            {
                var fkProperty = fkPropertyOnDependent ?? fkPropertyOnPrincipal;
                if (fkPropertiesOnNavigation.Count != 1
                    || !Equals(fkPropertiesOnNavigation.First(), fkProperty!.GetSimpleMemberName()))
                {
                    Dependencies.Logger.ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning(
                        fkPropertiesOnDependentToPrincipal != null
                            ? relationshipBuilder.Metadata.DependentToPrincipal!
                            : relationshipBuilder.Metadata.PrincipalToDependent!,
                        fkProperty!);

                    var newBuilder = SplitNavigationsToSeparateRelationships(relationshipBuilder);
                    if (newBuilder is null)
                    {
                        context.StopProcessing();
                        return null;
                    }

                    relationshipBuilder = newBuilder;
                    upgradePrincipalToDependentNavigationSource = false;

                    fkPropertiesToSet = fkPropertiesOnDependentToPrincipal
                        ?? new List<string> { fkPropertyOnDependent!.GetSimpleMemberName() };
                }

                if (fkPropertyOnDependent != null
                    || fkPropertiesOnDependentToPrincipal != null)
                {
                    upgradeDependentToPrincipalNavigationSource = true;
                }
                else
                {
                    shouldInvert = true;
                }
            }
        }

        var newRelationshipBuilder = relationshipBuilder;

        if (upgradeDependentToPrincipalNavigationSource)
        {
            newRelationshipBuilder = newRelationshipBuilder.HasNavigation(
                newRelationshipBuilder.Metadata.DependentToPrincipal!.Name, pointsToPrincipal: true, fromDataAnnotation: true)!;
        }

        if (upgradePrincipalToDependentNavigationSource)
        {
            newRelationshipBuilder = newRelationshipBuilder.HasNavigation(
                newRelationshipBuilder.Metadata.PrincipalToDependent!.Name, pointsToPrincipal: false, fromDataAnnotation: true)!;
        }

        if (shouldInvert)
        {
            newRelationshipBuilder = newRelationshipBuilder.HasEntityTypes(
                foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType, fromDataAnnotation: true)!;
        }
        else
        {
            var existingProperties = foreignKey.DeclaringEntityType.FindProperties(fkPropertiesToSet);
            if (existingProperties != null)
            {
                var conflictingFk = foreignKey.DeclaringEntityType.FindForeignKeys(existingProperties)
                    .FirstOrDefault(
                        fk => fk != foreignKey
                            && fk.PrincipalEntityType == foreignKey.PrincipalEntityType
                            && fk.GetConfigurationSource() == ConfigurationSource.DataAnnotation
                            && fk.GetPropertiesConfigurationSource() == ConfigurationSource.DataAnnotation);
                if (conflictingFk != null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.ConflictingForeignKeyAttributes(
                            existingProperties.Format(),
                            foreignKey.DeclaringEntityType.DisplayName(),
                            foreignKey.PrincipalEntityType.DisplayName()));
                }
            }
        }

        return newRelationshipBuilder?.HasForeignKey(fkPropertiesToSet, fromDataAnnotation: true);
    }

    private static IConventionForeignKeyBuilder? SplitNavigationsToSeparateRelationships(
        IConventionForeignKeyBuilder relationshipBuilder)
    {
        var foreignKey = relationshipBuilder.Metadata;
        var dependentToPrincipalNavigationName = foreignKey.DependentToPrincipal!.Name;
        var principalToDependentNavigationName = foreignKey.PrincipalToDependent!.Name;

        if (GetInversePropertyAttribute(foreignKey.PrincipalToDependent) != null
            || GetInversePropertyAttribute(foreignKey.DependentToPrincipal) != null)
        {
            // Relationship is joined by InversePropertyAttribute
            throw new InvalidOperationException(
                CoreStrings.InvalidRelationshipUsingDataAnnotations(
                    dependentToPrincipalNavigationName,
                    foreignKey.DeclaringEntityType.DisplayName(),
                    principalToDependentNavigationName,
                    foreignKey.PrincipalEntityType.DisplayName()));
        }

        return relationshipBuilder.HasNavigation((string?)null, pointsToPrincipal: false, fromDataAnnotation: true) is null
            ? null
            : foreignKey.PrincipalEntityType.Builder.HasRelationship(
                foreignKey.DeclaringEntityType,
                principalToDependentNavigationName,
                null,
                fromDataAnnotation: true)
            == null
                ? null
                : relationshipBuilder;
    }

    private static ForeignKeyAttribute? GetForeignKeyAttribute(IConventionNavigationBase navigation)
    {
        var memberInfo = navigation.GetIdentifyingMemberInfo();
        return memberInfo == null
            ? null
            : GetAttribute<ForeignKeyAttribute>(memberInfo);
    }

    private static InversePropertyAttribute? GetInversePropertyAttribute(IConventionNavigation navigation)
        => GetAttribute<InversePropertyAttribute>(navigation.GetIdentifyingMemberInfo());

    private static TAttribute? GetAttribute<TAttribute>(MemberInfo? memberInfo)
        where TAttribute : Attribute
        => memberInfo == null ? null : memberInfo.GetCustomAttribute<TAttribute>(inherit: true);

    [ContractAnnotation("navigation:null => null")]
    private MemberInfo? FindForeignKeyAttributeOnProperty(IConventionEntityType entityType, MemberInfo? navigation)
    {
        if (navigation == null)
        {
            return null;
        }

        var navigationName = navigation.GetSimpleMemberName();

        MemberInfo? candidateProperty = null;

        foreach (var memberInfo in entityType.GetRuntimeProperties().Values.Cast<MemberInfo>()
                     .Concat(entityType.GetRuntimeFields().Values))
        {
            if (!Attribute.IsDefined(memberInfo, typeof(ForeignKeyAttribute), inherit: true)
                || !entityType.Builder.CanHaveProperty(memberInfo, fromDataAnnotation: true))
            {
                continue;
            }

            var attribute = memberInfo.GetCustomAttribute<ForeignKeyAttribute>(inherit: true)!;
            if (attribute.Name != navigationName
                || (memberInfo is PropertyInfo propertyInfo
                    && IsNavigationCandidate(propertyInfo, entityType)))
            {
                continue;
            }

            if (candidateProperty != null)
            {
                throw new InvalidOperationException(
                    CoreStrings.CompositeFkOnProperty(navigationName, entityType.DisplayName()));
            }

            candidateProperty = memberInfo;
        }

        if (candidateProperty != null)
        {
            var fkAttributeOnNavigation = GetAttribute<ForeignKeyAttribute>(navigation);
            if (fkAttributeOnNavigation != null
                && fkAttributeOnNavigation.Name != candidateProperty.GetSimpleMemberName())
            {
                throw new InvalidOperationException(
                    CoreStrings.FkAttributeOnPropertyNavigationMismatch(
                        candidateProperty.Name, navigationName, entityType.DisplayName()));
            }
        }

        return candidateProperty;
    }

    private bool IsNavigationCandidate(PropertyInfo propertyInfo, IConventionEntityType entityType)
        => Dependencies.MemberClassifier.GetNavigationCandidates(entityType, useAttributes: true).TryGetValue(propertyInfo, out _);

    private static IReadOnlyList<string>? FindCandidateDependentPropertiesThroughNavigation(
        IConventionForeignKeyBuilder relationshipBuilder,
        bool pointsToPrincipal)
    {
        var navigation = pointsToPrincipal
            ? relationshipBuilder.Metadata.DependentToPrincipal
            : relationshipBuilder.Metadata.PrincipalToDependent!;

        var navigationFkAttribute = navigation != null
            ? GetForeignKeyAttribute(navigation)
            : null;

        if (navigationFkAttribute == null)
        {
            return null;
        }

        var properties = navigationFkAttribute.Name.Split(',').Select(p => p.Trim()).ToList();
        if (properties.Any(p => string.IsNullOrWhiteSpace(p) || p == navigation!.Name))
        {
            throw new InvalidOperationException(
                CoreStrings.InvalidPropertyListOnNavigation(
                    navigation!.Name, navigation.DeclaringEntityType.DisplayName(), navigationFkAttribute.Name));
        }

        var navigationPropertyTargetType =
            navigation!.DeclaringEntityType.GetRuntimeProperties()[navigation.Name].PropertyType;

        var otherNavigations = navigation.DeclaringEntityType.GetRuntimeProperties().Values
            .Where(p => p.PropertyType == navigationPropertyTargetType && p.GetSimpleMemberName() != navigation.Name)
            .OrderBy(p => p.GetSimpleMemberName());

        foreach (var propertyInfo in otherNavigations)
        {
            var attribute = GetAttribute<ForeignKeyAttribute>(propertyInfo);
            if (attribute?.Name == navigationFkAttribute.Name)
            {
                throw new InvalidOperationException(
                    CoreStrings.MultipleNavigationsSameFk(
                        navigation.DeclaringEntityType.DisplayName(),
                        attribute.Name,
                        $"'{navigation.Name}', '{propertyInfo.Name}'"));
            }
        }

        return properties;
    }

    /// <inheritdoc />
    public virtual void ProcessSkipNavigationForeignKeyChanged(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        IConventionForeignKey? foreignKey,
        IConventionForeignKey? oldForeignKey,
        IConventionContext<IConventionForeignKey> context)
    {
        if (foreignKey is { IsInModel: true })
        {
            var fkPropertiesToSet = FindCandidateDependentPropertiesThroughNavigation(skipNavigationBuilder.Metadata);
            if (fkPropertiesToSet != null)
            {
                foreignKey.Builder.HasForeignKey(fkPropertiesToSet, fromDataAnnotation: true);
            }
        }
    }

    private static IReadOnlyList<string>? FindCandidateDependentPropertiesThroughNavigation(
        IConventionSkipNavigation skipNavigation)
    {
        var navigationFkAttribute = GetForeignKeyAttribute(skipNavigation);
        if (navigationFkAttribute == null)
        {
            return null;
        }

        var properties = navigationFkAttribute.Name.Split(',').Select(p => p.Trim()).ToList();
        if (properties.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                CoreStrings.InvalidPropertyListOnNavigation(
                    skipNavigation.Name, skipNavigation.DeclaringEntityType.DisplayName(), navigationFkAttribute.Name));
        }

        return properties;
    }

    /// <inheritdoc />
    public virtual void ProcessPropertyAdded(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<IConventionPropertyBuilder> context)
    {
        var property = propertyBuilder.Metadata;
        var member = property.GetIdentifyingMemberInfo();
        if (member != null
            && Attribute.IsDefined(member, typeof(ForeignKeyAttribute), inherit: true)
            && property.DeclaringType is IConventionComplexType)
        {
            throw new InvalidOperationException(
                CoreStrings.AttributeNotOnEntityTypeProperty(
                    "ForeignKey", property.DeclaringType.DisplayName(), property.Name));
        }
    }

    /// <inheritdoc />
    public virtual void ProcessComplexPropertyAdded(
        IConventionComplexPropertyBuilder propertyBuilder,
        IConventionContext<IConventionComplexPropertyBuilder> context)
    {
        var property = propertyBuilder.Metadata;
        var member = property.GetIdentifyingMemberInfo();
        if (member != null
            && Attribute.IsDefined(member, typeof(ForeignKeyAttribute), inherit: true))
        {
            throw new InvalidOperationException(
                CoreStrings.AttributeNotOnEntityTypeProperty(
                    "ForeignKey", property.DeclaringType.DisplayName(), property.Name));
        }
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var declaredNavigation in entityType.GetDeclaredNavigations())
            {
                if (declaredNavigation.IsCollection)
                {
                    var foreignKey = declaredNavigation.ForeignKey;
                    var fkPropertyOnPrincipal
                        = FindForeignKeyAttributeOnProperty(
                            foreignKey.PrincipalEntityType, declaredNavigation.GetIdentifyingMemberInfo());
                    if (fkPropertyOnPrincipal != null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.FkAttributeOnNonUniquePrincipal(
                                declaredNavigation.Name,
                                foreignKey.PrincipalEntityType.DisplayName(),
                                foreignKey.DeclaringEntityType.DisplayName()));
                    }
                }
            }
        }
    }
}
