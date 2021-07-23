// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     <para>
    ///         A convention that configures the foreign key properties associated with a navigation property
    ///         based on the <see cref="ForeignKeyAttribute" /> specified on the properties or the navigation properties.
    ///     </para>
    ///     <para>
    ///         For one-to-one relationships the attribute has to be specified on the navigation property pointing to the principal.
    ///     </para>
    /// </summary>
    public class ForeignKeyAttributeConvention : IForeignKeyAddedConvention, INavigationAddedConvention, IModelFinalizingConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="ForeignKeyAttributeConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        public ForeignKeyAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        {
            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

        /// <summary>
        ///     Called after a foreign key is added to the entity type.
        /// </summary>
        /// <param name="relationshipBuilder"> The builder for the foreign key. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
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
        /// <param name="navigationBuilder"> The builder for the navigation. </param>
        /// <param name="context"> Additional information associated with convention execution. </param>
        public virtual void ProcessNavigationAdded(
            IConventionNavigationBuilder navigationBuilder,
            IConventionContext<IConventionNavigationBuilder> context)
        {
            Check.NotNull(navigationBuilder, nameof(navigationBuilder));

            var onDependent = navigationBuilder.Metadata.IsOnDependent;
            var newRelationshipBuilder =
                UpdateRelationshipBuilder(navigationBuilder.Metadata.ForeignKey.Builder, context);
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
                = FindForeignKeyAttributeOnProperty(foreignKey.PrincipalEntityType, foreignKey.PrincipalToDependent?.Name);

            var fkPropertyOnDependent
                = FindForeignKeyAttributeOnProperty(foreignKey.DeclaringEntityType, foreignKey.DependentToPrincipal?.Name);

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

            return newRelationshipBuilder.HasForeignKey(fkPropertiesToSet, fromDataAnnotation: true);
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

        private static ForeignKeyAttribute? GetForeignKeyAttribute(IConventionTypeBase entityType, string propertyName)
            => entityType.GetRuntimeProperties().Values
                .FirstOrDefault(
                    p => string.Equals(p.GetSimpleMemberName(), propertyName, StringComparison.OrdinalIgnoreCase)
                        && Attribute.IsDefined(p, typeof(ForeignKeyAttribute), inherit: true))
                ?.GetCustomAttribute<ForeignKeyAttribute>(inherit: true);

        private static ForeignKeyAttribute? GetForeignKeyAttribute(IConventionNavigationBase navigation)
            => GetAttribute<ForeignKeyAttribute>(navigation.GetIdentifyingMemberInfo());

        private static InversePropertyAttribute? GetInversePropertyAttribute(IConventionNavigation navigation)
            => GetAttribute<InversePropertyAttribute>(navigation.GetIdentifyingMemberInfo());

        private static TAttribute? GetAttribute<TAttribute>(MemberInfo? memberInfo)
            where TAttribute : Attribute
        {
            if (memberInfo == null
                || !Attribute.IsDefined(memberInfo, typeof(TAttribute), inherit: true))
            {
                return null;
            }

            return memberInfo.GetCustomAttribute<TAttribute>(inherit: true);
        }

        [ContractAnnotation("navigationName:null => null")]
        private MemberInfo? FindForeignKeyAttributeOnProperty(IConventionEntityType entityType, string? navigationName)
        {
            if (string.IsNullOrWhiteSpace(navigationName))
            {
                return null;
            }

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
                var fkAttributeOnNavigation = GetForeignKeyAttribute(entityType, navigationName);
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
            => Dependencies.MemberClassifier.GetNavigationCandidates(entityType).TryGetValue(propertyInfo, out var _);

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

            if (navigationFkAttribute != null)
            {
                var properties = navigationFkAttribute.Name.Split(',').Select(p => p.Trim()).ToList();

                if (properties.Any(string.IsNullOrWhiteSpace))
                {
                    throw new InvalidOperationException(
                        CoreStrings.InvalidPropertyListOnNavigation(navigation!.Name, navigation.DeclaringEntityType.DisplayName()));
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

            return null;
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
                            = FindForeignKeyAttributeOnProperty(foreignKey.PrincipalEntityType, declaredNavigation.Name);
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

                foreach (var declaredSkipNavigation in entityType.GetDeclaredSkipNavigations())
                {
                    var fkAttribute = GetForeignKeyAttribute(declaredSkipNavigation);
                    if (fkAttribute != null
                        && declaredSkipNavigation.ForeignKey?.GetPropertiesConfigurationSource() != ConfigurationSource.Explicit)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.FkAttributeOnSkipNavigation(entityType.DisplayName(), declaredSkipNavigation.Name));
                    }
                }
            }
        }
    }
}
