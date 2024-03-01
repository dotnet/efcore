// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the inverse navigation property based on the <see cref="InversePropertyAttribute" />
///     specified on the other navigation property.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class InversePropertyAttributeConvention :
    NavigationAttributeConventionBase<InversePropertyAttribute>,
    IEntityTypeAddedConvention,
    IEntityTypeRemovedConvention,
    IEntityTypeBaseTypeChangedConvention,
    IEntityTypeMemberIgnoredConvention,
    INavigationAddedConvention,
    IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="InversePropertyAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public InversePropertyAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Called for every navigation property that has an attribute after an entity type is added to the model.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="navigationMemberInfo">The navigation member info.</param>
    /// <param name="targetClrType">The CLR type of the target entity type</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public override void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        MemberInfo navigationMemberInfo,
        Type targetClrType,
        InversePropertyAttribute attribute,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => Process(entityTypeBuilder, navigationMemberInfo, targetClrType, attribute);

    private void Process(
        IConventionEntityTypeBuilder entityTypeBuilder,
        MemberInfo navigationMemberInfo,
        Type targetClrType,
        InversePropertyAttribute attribute)
    {
        if (!entityTypeBuilder.CanHaveNavigation(navigationMemberInfo, fromDataAnnotation: true))
        {
            return;
        }

        var targetEntityTypeBuilder = TryGetTargetEntityTypeBuilder(entityTypeBuilder, targetClrType, navigationMemberInfo);
        if (targetEntityTypeBuilder == null)
        {
            return;
        }

        ConfigureInverseNavigation(entityTypeBuilder, navigationMemberInfo, targetEntityTypeBuilder, attribute);
    }

    private IConventionForeignKeyBuilder? ConfigureInverseNavigation(
        IConventionEntityTypeBuilder entityTypeBuilder,
        MemberInfo navigationMemberInfo,
        IConventionEntityTypeBuilder targetEntityTypeBuilder,
        InversePropertyAttribute attribute)
    {
        var entityType = entityTypeBuilder.Metadata;
        var targetEntityType = targetEntityTypeBuilder.Metadata;
        var targetClrType = targetEntityType.ClrType;
        var navigationCandidates = Dependencies.MemberClassifier.GetNavigationCandidates(targetEntityType, useAttributes: true);
        var inverseNavigationPropertyInfo = targetEntityType.GetRuntimeProperties().Values
                .FirstOrDefault(
                    p => string.Equals(p.GetSimpleMemberName(), attribute.Property, StringComparison.Ordinal)
                        && navigationCandidates.ContainsKey(p))
            ?? targetEntityType.GetRuntimeProperties().Values
                .FirstOrDefault(
                    p => string.Equals(p.GetSimpleMemberName(), attribute.Property, StringComparison.OrdinalIgnoreCase)
                        && navigationCandidates.ContainsKey(p));

        if (inverseNavigationPropertyInfo == null
            || !navigationCandidates[inverseNavigationPropertyInfo].Type.IsAssignableFrom(entityType.ClrType))
        {
            throw new InvalidOperationException(
                CoreStrings.InvalidNavigationWithInverseProperty(
                    navigationMemberInfo.Name, entityType.DisplayName(), attribute.Property, targetClrType.ShortDisplayName()));
        }

        if (Equals(inverseNavigationPropertyInfo, navigationMemberInfo))
        {
            throw new InvalidOperationException(
                CoreStrings.SelfReferencingNavigationWithInverseProperty(
                    entityType.DisplayName(),
                    navigationMemberInfo.Name));
        }

        // Check for InversePropertyAttribute on the inverse navigation to verify that it matches.
        if (Attribute.IsDefined(inverseNavigationPropertyInfo, typeof(InversePropertyAttribute)))
        {
            var inverseAttribute = inverseNavigationPropertyInfo.GetCustomAttribute<InversePropertyAttribute>(true)!;
            if (inverseAttribute.Property != navigationMemberInfo.GetSimpleMemberName())
            {
                throw new InvalidOperationException(
                    CoreStrings.InversePropertyMismatch(
                        navigationMemberInfo.Name,
                        entityType.DisplayName(),
                        inverseNavigationPropertyInfo.Name,
                        targetEntityType.DisplayName()));
            }
        }

        var referencingNavigationsWithAttribute =
            AddInverseNavigation(entityType, navigationMemberInfo, targetEntityType, inverseNavigationPropertyInfo);

        if (TryRemoveIfAmbiguous(
                entityType,
                navigationMemberInfo,
                targetEntityType,
                targetEntityType.BaseType,
                inverseNavigationPropertyInfo,
                referencingNavigationsWithAttribute,
                out var conventionForeignKeyBuilder))
        {
            return conventionForeignKeyBuilder;
        }

        var ownership = entityType.FindOwnership();
        if (ownership != null
            && ownership.PrincipalEntityType == targetEntityType
            && ownership.PrincipalToDependent?.GetIdentifyingMemberInfo() != inverseNavigationPropertyInfo)
        {
            Dependencies.Logger.NonOwnershipInverseNavigationWarning(
                entityType, navigationMemberInfo,
                targetEntityType, inverseNavigationPropertyInfo,
                ownership.PrincipalToDependent?.GetIdentifyingMemberInfo()!);

            return null;
        }

        var targetOwnership = targetEntityType.FindOwnership();
        if (targetOwnership != null
            && targetOwnership.PrincipalEntityType == entityType
            && targetOwnership.PrincipalToDependent?.GetIdentifyingMemberInfo() != navigationMemberInfo)
        {
            Dependencies.Logger.NonOwnershipInverseNavigationWarning(
                entityType, navigationMemberInfo,
                targetEntityType, inverseNavigationPropertyInfo,
                targetOwnership.PrincipalToDependent?.GetIdentifyingMemberInfo()!);

            return null;
        }

        if (targetEntityType.IsOwned()
            && (targetOwnership == null
                || targetOwnership.PrincipalEntityType == entityType))
        {
            if (navigationMemberInfo.DeclaringType != entityType.ClrType
                && (entityType.Model.FindEntityType(navigationMemberInfo.DeclaringType!) != null
                    || (navigationMemberInfo.DeclaringType != entityType.ClrType.BaseType
                        && entityType.Model.FindEntityType(entityType.ClrType.BaseType!) != null)))
            {
                return null;
            }

            return entityTypeBuilder.HasOwnership(
                targetEntityType,
                navigationMemberInfo,
                inverseNavigationPropertyInfo,
                fromDataAnnotation: true);
        }

        if (entityType.IsOwned()
            && (ownership == null
                || ownership.PrincipalEntityType == targetEntityType))
        {
            if (navigationMemberInfo.DeclaringType != entityType.ClrType
                && (entityType.Model.FindEntityType(navigationMemberInfo.DeclaringType!) != null
                    || (navigationMemberInfo.DeclaringType != entityType.ClrType.BaseType
                        && entityType.Model.FindEntityType(entityType.ClrType.BaseType!) != null)))
            {
                return null;
            }

            return targetEntityTypeBuilder.HasOwnership(
                entityTypeBuilder.Metadata,
                inverseNavigationPropertyInfo,
                navigationMemberInfo,
                fromDataAnnotation: true);
        }

        if (ownership != null
            || targetOwnership != null)
        {
            return null;
        }

        var newForeignKeyBuilder = targetEntityTypeBuilder.HasRelationship(
            entityType,
            inverseNavigationPropertyInfo,
            navigationMemberInfo,
            fromDataAnnotation: true);

        if (newForeignKeyBuilder == null
            && navigationMemberInfo is PropertyInfo navigationPropertyInfo)
        {
            var navigationTargetType = navigationPropertyInfo.PropertyType.TryGetSequenceType();
            var inverseNavigationTargetType = inverseNavigationPropertyInfo.PropertyType.TryGetSequenceType();
            if (navigationTargetType != null
                && inverseNavigationTargetType != null
                && navigationTargetType.IsAssignableFrom(targetClrType)
                && inverseNavigationTargetType.IsAssignableFrom(entityType.ClrType))
            {
                entityTypeBuilder.HasSkipNavigation(
                    navigationPropertyInfo, targetEntityType,
                    inverseNavigationPropertyInfo, collections: true, onDependent: false, fromDataAnnotation: true);
            }
        }

        return newForeignKeyBuilder;
    }

    private static bool TryRemoveIfAmbiguous(
        IConventionEntityType entityType,
        MemberInfo navigationMemberInfo,
        IConventionEntityType targetEntityType,
        IConventionEntityType? targetBaseType,
        MemberInfo inverseNavigationMemberInfo,
        List<(MemberInfo, IConventionEntityType)> referencingNavigationsWithAttribute,
        out IConventionForeignKeyBuilder? remainingInverseNavigation)
    {
        var ambiguousInverse = FindAmbiguousInverse(
            navigationMemberInfo, entityType, referencingNavigationsWithAttribute);
        while (ambiguousInverse == null
               && targetBaseType != null)
        {
            var navigationMap = GetInverseNavigations(targetBaseType);
            if (navigationMap != null
                && navigationMap.TryGetValue(inverseNavigationMemberInfo.Name, out var inverseTuple))
            {
                referencingNavigationsWithAttribute = inverseTuple.References;
                ambiguousInverse = FindAmbiguousInverse(navigationMemberInfo, entityType, referencingNavigationsWithAttribute);
            }

            targetBaseType = targetBaseType.BaseType;
        }

        if (ambiguousInverse != null)
        {
            if (entityType.FindSkipNavigation(navigationMemberInfo) is IConventionSkipNavigation existingSkipNavigation)
            {
                var existingSkipNavigationInverse = existingSkipNavigation.Inverse;
                var inverseSkipNavigation = targetEntityType.FindSkipNavigation(inverseNavigationMemberInfo);
                var existingInverse = inverseSkipNavigation?.Inverse;
                var existingInverseType = existingInverse?.DeclaringEntityType;
                if (existingInverse != null
                    && IsAmbiguousInverse(
                        existingInverse.GetIdentifyingMemberInfo()!, existingInverseType!, referencingNavigationsWithAttribute))
                {
                    existingInverse.DeclaringEntityType.Builder.HasNoSkipNavigation(existingInverse, fromDataAnnotation: true);
                    inverseSkipNavigation!.DeclaringEntityType.Builder.HasNoSkipNavigation(
                        inverseSkipNavigation, fromDataAnnotation: true);
                }

                if (existingSkipNavigation.IsInModel)
                {
                    entityType.Builder.HasNoSkipNavigation(existingSkipNavigation, fromDataAnnotation: true);
                }

                if (existingSkipNavigationInverse?.IsInModel == true)
                {
                    existingSkipNavigationInverse.DeclaringEntityType.Builder
                        .HasNoSkipNavigation(existingSkipNavigationInverse, fromDataAnnotation: true);
                }

                var existingAmbiguousNavigation = FindActualEntityType(ambiguousInverse.Value.Item2)!
                    .FindSkipNavigation(ambiguousInverse.Value.Item1);

                existingAmbiguousNavigation?.DeclaringEntityType.Builder.HasNoSkipNavigation(
                    existingAmbiguousNavigation, fromDataAnnotation: true);

                remainingInverseNavigation = entityType.FindSkipNavigation(navigationMemberInfo)?.ForeignKey!.Builder;
                return true;
            }
            else
            {
                var existingInverse = targetEntityType.FindNavigation(inverseNavigationMemberInfo)?.Inverse;
                if (existingInverse != null
                    && IsAmbiguousInverse(
                        existingInverse.GetIdentifyingMemberInfo()!,
                        existingInverse.DeclaringEntityType,
                        referencingNavigationsWithAttribute))
                {
                    Remove(existingInverse);
                }

                var existingNavigation = entityType.FindNavigation(navigationMemberInfo);
                if (existingNavigation != null)
                {
                    Remove(existingNavigation);
                }

                var existingAmbiguousNavigation = FindActualEntityType(ambiguousInverse.Value.Item2)!
                    .FindNavigation(ambiguousInverse.Value.Item1);
                if (existingAmbiguousNavigation != null)
                {
                    Remove(existingAmbiguousNavigation);
                }

                remainingInverseNavigation = entityType.FindNavigation(navigationMemberInfo)?.ForeignKey.Builder;
                return true;
            }
        }

        remainingInverseNavigation = null;
        return false;
    }

    private static void Remove(IConventionNavigation navigation)
    {
        var foreignKey = navigation.ForeignKey;
        if (foreignKey.IsOwnership)
        {
            if (navigation.IsOnDependent)
            {
                foreignKey.Builder.HasNavigation(
                    (string?)null,
                    navigation.IsOnDependent,
                    fromDataAnnotation: true);
            }
            else if (ConfigurationSource.DataAnnotation.Overrides(foreignKey.DeclaringEntityType.GetConfigurationSource()))
            {
                navigation.DeclaringEntityType.Model.Builder.HasNoEntityType(foreignKey.DeclaringEntityType, fromDataAnnotation: true);
            }
            else
            {
                foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, fromDataAnnotation: true);
            }
        }
        else if (foreignKey.DeclaringEntityType.Builder.HasNoRelationship(foreignKey, fromDataAnnotation: true) == null)
        {
            foreignKey.Builder.HasNavigation(
                (string?)null,
                navigation.IsOnDependent,
                fromDataAnnotation: true);
        }
    }

    /// <inheritdoc />
    public override void ProcessEntityTypeRemoved(
        IConventionModelBuilder modelBuilder,
        IConventionEntityType entityType,
        MemberInfo navigationMemberInfo,
        Type targetClrType,
        InversePropertyAttribute attribute,
        IConventionContext<IConventionEntityType> context)
    {
        var targetEntityType = modelBuilder.Metadata.FindEntityType(targetClrType);
        if (targetEntityType != null)
        {
            RemoveInverseNavigation(entityType, navigationMemberInfo, targetEntityType, attribute.Property);
        }

        var declaringType = navigationMemberInfo.DeclaringType;
        Check.DebugAssert(declaringType != null, "declaringType is null");
        if (modelBuilder.Metadata.FindEntityType(declaringType) != null
            || entityType.HasSharedClrType
            || entityType.IsOwned())
        {
            return;
        }

        var navigationName = navigationMemberInfo.GetSimpleMemberName();
        var leastDerivedEntityTypes = modelBuilder.Metadata.FindLeastDerivedEntityTypes(
            declaringType, t => !t.HasSharedClrType && !t.IsOwned());
        foreach (var leastDerivedEntityType in leastDerivedEntityTypes)
        {
            if (leastDerivedEntityType.Builder.IsIgnored(navigationName, fromDataAnnotation: true))
            {
                continue;
            }

            Process(leastDerivedEntityType.Builder, navigationMemberInfo, targetClrType, attribute);
        }
    }

    /// <inheritdoc />
    public override void ProcessNavigationAdded(
        IConventionNavigationBuilder navigationBuilder,
        InversePropertyAttribute attribute,
        IConventionContext<IConventionNavigationBuilder> context)
    {
        var navigation = navigationBuilder.Metadata;
        var foreignKey = navigation.ForeignKey;
        if (foreignKey.DeclaringEntityType.IsOwned()
            || foreignKey.PrincipalEntityType.IsOwned())
        {
            return;
        }

        var newRelationshipBuilder = ConfigureInverseNavigation(
            navigation.DeclaringEntityType.Builder,
            navigation.GetIdentifyingMemberInfo()!,
            navigation.TargetEntityType.Builder,
            attribute);

        if (newRelationshipBuilder == null)
        {
            context.StopProcessing();
            return;
        }

        var newNavigation = navigation.IsOnDependent
            ? newRelationshipBuilder.Metadata.DependentToPrincipal
            : newRelationshipBuilder.Metadata.PrincipalToDependent;

        context.StopProcessingIfChanged(newNavigation?.Builder);
    }

    /// <inheritdoc />
    public override void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        MemberInfo navigationMemberInfo,
        Type targetClrType,
        InversePropertyAttribute attribute,
        IConventionContext<IConventionEntityType> context)
    {
        var entityType = entityTypeBuilder.Metadata;
        if (navigationMemberInfo.DeclaringType != entityType.ClrType)
        {
            if (newBaseType == null)
            {
                Process(entityTypeBuilder, navigationMemberInfo, targetClrType, attribute);
            }
            else
            {
                var targetEntityType = entityType.Model.FindEntityType(targetClrType);
                if (targetEntityType == null)
                {
                    return;
                }

                RemoveInverseNavigation(entityType, navigationMemberInfo, targetEntityType, attribute.Property);
            }
        }
    }

    /// <inheritdoc />
    public override void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        base.ProcessEntityTypeBaseTypeChanged(
            entityTypeBuilder,
            newBaseType,
            oldBaseType,
            context);

        if (newBaseType == null)
        {
            return;
        }

        foreach (var entityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
        {
            var inverseNavigations = GetInverseNavigations(entityType);
            if (inverseNavigations == null)
            {
                continue;
            }

            foreach (var (navigation, references) in inverseNavigations.Values)
            {
                foreach (var (memberInfo, conventionEntityType) in references)
                {
                    if (TryRemoveIfAmbiguous(
                            conventionEntityType,
                            memberInfo,
                            entityType,
                            newBaseType,
                            navigation,
                            references,
                            out _))
                    {
                        break;
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    public override void ProcessEntityTypeMemberIgnored(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionContext<string> context)
    {
        base.ProcessEntityTypeMemberIgnored(entityTypeBuilder, name, context);

        var entityType = entityTypeBuilder.Metadata;
        var navigationPropertyInfo = entityType.GetRuntimeProperties().Find(name);
        if (navigationPropertyInfo == null)
        {
            return;
        }

        RemoveInverseNavigation(null, null, entityType, name);
    }

    /// <inheritdoc />
    public override void ProcessEntityTypeMemberIgnored(
        IConventionEntityTypeBuilder entityTypeBuilder,
        MemberInfo navigationMemberInfo,
        Type targetClrType,
        InversePropertyAttribute attribute,
        IConventionContext<string> context)
    {
        var targetEntityType = TryGetTargetEntityTypeBuilder(
            entityTypeBuilder,
            targetClrType, navigationMemberInfo, shouldCreate: false)?.Metadata;
        if (targetEntityType == null)
        {
            return;
        }

        RemoveInverseNavigation(entityTypeBuilder.Metadata, navigationMemberInfo, targetEntityType, attribute.Property);
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        var model = modelBuilder.Metadata;
        foreach (var entityType in model.GetEntityTypes())
        {
            var inverseNavigations = GetInverseNavigations(entityType);
            if (inverseNavigations == null)
            {
                continue;
            }

            foreach (var (navigation, references) in inverseNavigations.Values)
            {
                foreach (var (memberInfo, conventionEntityType) in references)
                {
                    var ambiguousInverse = FindAmbiguousInverse(
                        memberInfo,
                        conventionEntityType,
                        references);

                    var baseType = entityType.BaseType;
                    while (ambiguousInverse == null
                           && baseType != null)
                    {
                        var navigationMap = GetInverseNavigations(baseType);
                        if (navigationMap != null
                            && navigationMap.TryGetValue(navigation.Name, out var inverseTuple))
                        {
                            var referencingNavigationsWithAttribute = inverseTuple.References;
                            ambiguousInverse = FindAmbiguousInverse(
                                memberInfo,
                                conventionEntityType,
                                referencingNavigationsWithAttribute);
                        }

                        baseType = baseType.BaseType;
                    }

                    if (ambiguousInverse != null)
                    {
                        Dependencies.Logger.MultipleInversePropertiesSameTargetWarning(
                            new[]
                            {
                                Tuple.Create<MemberInfo?, Type>(
                                    memberInfo, conventionEntityType.ClrType),
                                Tuple.Create<MemberInfo?, Type>(ambiguousInverse.Value.Item1, ambiguousInverse.Value.Item2.ClrType)
                            },
                            navigation,
                            entityType.ClrType);
                        break;
                    }
                }
            }
        }

        foreach (var entityType in model.GetEntityTypes())
        {
            entityType.RemoveAnnotation(CoreAnnotationNames.InverseNavigations);
        }
    }

    /// <summary>
    ///     Returns a value indication whether the given navigation has ambiguous inverse navigations with
    ///     <see cref="InversePropertyAttribute" />.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="navigation">The navigation.</param>
    /// <param name="targetEntityType">Target entity type.</param>
    /// <returns>
    ///     <see langword="true" /> if the given navigation has ambiguous inverse navigations with <see cref="InversePropertyAttribute" />.
    /// </returns>
    public static bool IsAmbiguous(
        IConventionEntityType entityType,
        MemberInfo navigation,
        IConventionEntityType targetEntityType)
    {
        if (!Attribute.IsDefined(navigation, typeof(InversePropertyAttribute)))
        {
            return false;
        }

        var currentEntityType = targetEntityType;
        while (currentEntityType != null)
        {
            var navigationMap = GetInverseNavigations(currentEntityType);
            if (navigationMap != null)
            {
                foreach (var (memberInfo, references) in navigationMap.Values)
                {
                    if (memberInfo.GetMemberType().IsAssignableFrom(entityType.ClrType)
                        && IsAmbiguousInverse(navigation, entityType, references))
                    {
                        return true;
                    }
                }
            }

            currentEntityType = currentEntityType.BaseType;
        }

        return false;
    }

    private static bool IsAmbiguousInverse(
        MemberInfo navigation,
        IConventionEntityType entityType,
        List<(MemberInfo, IConventionEntityType)> referencingNavigationsWithAttribute)
        => FindAmbiguousInverse(navigation, entityType, referencingNavigationsWithAttribute) != null;

    private static (MemberInfo, IConventionEntityType)? FindAmbiguousInverse(
        MemberInfo navigation,
        IConventionEntityType entityType,
        List<(MemberInfo Inverse, IConventionEntityType InverseEntityType)> referencingNavigationsWithAttribute)
    {
        (MemberInfo, IConventionEntityType)? ambiguousTuple = null;
        foreach (var referencingTuple in referencingNavigationsWithAttribute)
        {
            var inverseEntityType = FindActualEntityType(referencingTuple.InverseEntityType);
            if (inverseEntityType is null
                || inverseEntityType.Builder.IsIgnored(referencingTuple.Inverse.GetSimpleMemberName(), fromDataAnnotation: true))
            {
                continue;
            }

            if (!referencingTuple.Inverse.IsSameAs(navigation)
                || (!entityType.IsAssignableFrom(inverseEntityType)
                    && !inverseEntityType.IsAssignableFrom(entityType)))
            {
                ambiguousTuple = referencingTuple;
                break;
            }
        }

        return ambiguousTuple;
    }

    private static List<(MemberInfo, IConventionEntityType)> AddInverseNavigation(
        IConventionEntityType entityType,
        MemberInfo navigation,
        IConventionEntityType targetEntityType,
        MemberInfo inverseNavigation)
    {
        var inverseNavigations = GetInverseNavigations(targetEntityType);
        if (inverseNavigations == null)
        {
            inverseNavigations = new Dictionary<string, (MemberInfo, List<(MemberInfo, IConventionEntityType)>)>();
            SetInverseNavigations(targetEntityType.Builder, inverseNavigations);
        }

        List<(MemberInfo, IConventionEntityType)> referencingNavigationsWithAttribute;
        if (!inverseNavigations.TryGetValue(inverseNavigation.Name, out var inverseTuple))
        {
            referencingNavigationsWithAttribute = [];
            inverseNavigations[inverseNavigation.Name] = (inverseNavigation, referencingNavigationsWithAttribute);
        }
        else
        {
            referencingNavigationsWithAttribute = inverseTuple.References;
        }

        foreach (var (memberInfo, conventionEntityType) in referencingNavigationsWithAttribute)
        {
            if (memberInfo.IsSameAs(navigation)
                && conventionEntityType.ClrType == entityType.ClrType
                && FindActualEntityType(conventionEntityType) == entityType)
            {
                return referencingNavigationsWithAttribute;
            }
        }

        referencingNavigationsWithAttribute.Add((navigation, entityType));

        return referencingNavigationsWithAttribute;
    }

    private static void RemoveInverseNavigation(
        IConventionEntityType? declaringEntityType,
        MemberInfo? navigation,
        IConventionEntityType targetEntityType,
        string inverseNavigationName)
    {
        var declaringType = declaringEntityType?.ClrType;
        var inverseNavigations = GetInverseNavigations(targetEntityType);
        if (inverseNavigations == null
            || !inverseNavigations.TryGetValue(inverseNavigationName, out var inverseNavigationPair))
        {
            return;
        }

        var anyRemoved = false;
        var (inverseNavigation, referencingNavigationsWithAttribute) = inverseNavigationPair;
        for (var index = 0; index < referencingNavigationsWithAttribute.Count; index++)
        {
            var (memberInfo, conventionEntityType) = referencingNavigationsWithAttribute[index];
            if (navigation == null)
            {
                anyRemoved = true;
                referencingNavigationsWithAttribute.RemoveAt(index--);
                if (referencingNavigationsWithAttribute.Count == 0)
                {
                    inverseNavigations.Remove(inverseNavigation.Name);
                }

                var otherEntityType = FindActualEntityType(conventionEntityType);
                if (otherEntityType != null)
                {
                    // TODO: Rely on layering to trigger relationship discovery instead #15898

                    var existingInverses = targetEntityType.GetNavigations()
                        .Where(n => n.TargetEntityType == otherEntityType).ToList();

                    if (existingInverses.Count == 0)
                    {
                        otherEntityType.Builder.HasRelationship(
                            targetEntityType,
                            memberInfo,
                            null);
                    }
                    else if (existingInverses.Count == 1)
                    {
                        var existingInverse = existingInverses[0];
                        if (existingInverse.Inverse == null)
                        {
                            // TODO: Rely on layering instead of using DataAnnotation configuration source
                            // to override the null navigation configuration #15898
                            otherEntityType.Builder.HasRelationship(
                                targetEntityType,
                                memberInfo,
                                existingInverse.PropertyInfo,
                                fromDataAnnotation: true);
                        }
                        else
                        {
                            otherEntityType.Builder.HasRelationship(
                                targetEntityType,
                                memberInfo,
                                null);
                        }
                    }
                }
            }
            else if (memberInfo.IsSameAs(navigation)
                     && ((!conventionEntityType.IsInModel
                             && declaringType!.IsAssignableFrom(conventionEntityType.ClrType))
                         || (conventionEntityType.IsInModel
                             && declaringEntityType!.IsAssignableFrom(conventionEntityType))))
            {
                anyRemoved = true;
                referencingNavigationsWithAttribute.RemoveAt(index--);
                if (referencingNavigationsWithAttribute.Count == 0)
                {
                    inverseNavigations.Remove(inverseNavigation.Name);
                }
            }
        }

        if (anyRemoved
            && referencingNavigationsWithAttribute.Count == 1)
        {
            var otherEntityType = FindActualEntityType(referencingNavigationsWithAttribute[0].Item2);
            if (otherEntityType != null)
            {
                targetEntityType.Builder.HasRelationship(
                    otherEntityType,
                    inverseNavigation,
                    referencingNavigationsWithAttribute[0].Item1,
                    fromDataAnnotation: true);
            }
        }
    }

    private static IConventionEntityType? FindActualEntityType(IConventionEntityType entityType)
        => ((Model)entityType.Model).FindActualEntityType((EntityType)entityType);

    /// <summary>
    ///     Finds or tries to create an entity type target for the given navigation member.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the referencing entity type.</param>
    /// <param name="targetClrType">The CLR type of the target entity type.</param>
    /// <param name="navigationMemberInfo">The navigation member.</param>
    /// <param name="shouldCreate">Whether an entity type should be created if one doesn't currently exist.</param>
    /// <returns>The builder for the target entity type or <see langword="null" /> if it can't be created.</returns>
    protected virtual IConventionEntityTypeBuilder? TryGetTargetEntityTypeBuilder(
        IConventionEntityTypeBuilder entityTypeBuilder,
        Type targetClrType,
        MemberInfo navigationMemberInfo,
        bool shouldCreate = true)
        => entityTypeBuilder
            .GetTargetEntityTypeBuilder(
                targetClrType,
                navigationMemberInfo,
                shouldCreate,
                fromDataAnnotation: true);

    private static Dictionary<string, (MemberInfo Navigation, List<(MemberInfo, IConventionEntityType)> References)>?
        GetInverseNavigations(
            IConventionAnnotatable entityType)
        => entityType.FindAnnotation(CoreAnnotationNames.InverseNavigations)?.Value
            as Dictionary<string, (MemberInfo, List<(MemberInfo, IConventionEntityType)>)>;

    private static void SetInverseNavigations(
        IConventionAnnotatableBuilder entityTypeBuilder,
        Dictionary<string, (MemberInfo, List<(MemberInfo, IConventionEntityType)>)> inverseNavigations)
        => entityTypeBuilder.HasAnnotation(CoreAnnotationNames.InverseNavigations, inverseNavigations);
}
