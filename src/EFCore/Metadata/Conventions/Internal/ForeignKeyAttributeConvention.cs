// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ForeignKeyAttributeConvention : IForeignKeyAddedConvention, IModelBuiltConvention
    {
        private readonly IMemberClassifier _memberClassifier;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Model> _logger;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ForeignKeyAttributeConvention(
            [NotNull] IMemberClassifier memberClassifier,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            Check.NotNull(memberClassifier, nameof(memberClassifier));

            _memberClassifier = memberClassifier;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            Check.NotNull(relationshipBuilder, nameof(relationshipBuilder));

            var foreignKey = relationshipBuilder.Metadata;

            var fkPropertyOnPrincipal
                = FindForeignKeyAttributeOnProperty(foreignKey.PrincipalEntityType, foreignKey.PrincipalToDependent?.Name);

            var fkPropertyOnDependent
                = FindForeignKeyAttributeOnProperty(foreignKey.DeclaringEntityType, foreignKey.DependentToPrincipal?.Name);

            if (fkPropertyOnDependent != null
                && fkPropertyOnPrincipal != null)
            {
                _logger.ForeignKeyAttributesOnBothPropertiesWarning(
                    foreignKey.PrincipalToDependent,
                    foreignKey.DependentToPrincipal,
                    fkPropertyOnPrincipal,
                    fkPropertyOnDependent);

                relationshipBuilder = SplitNavigationsToSeparateRelationships(relationshipBuilder);
                if (relationshipBuilder == null)
                {
                    return null;
                }

                fkPropertyOnPrincipal = null;
            }

            var fkPropertiesOnPrincipalToDependent
                = FindCandidateDependentPropertiesThroughNavigation(relationshipBuilder, pointsToPrincipal: false);

            var fkPropertiesOnDependentToPrincipal
                = FindCandidateDependentPropertiesThroughNavigation(relationshipBuilder, pointsToPrincipal: true);

            if (fkPropertiesOnDependentToPrincipal != null
                && fkPropertiesOnPrincipalToDependent != null)
            {
                _logger.ForeignKeyAttributesOnBothNavigationsWarning(
                    relationshipBuilder.Metadata.DependentToPrincipal, relationshipBuilder.Metadata.PrincipalToDependent);

                relationshipBuilder = SplitNavigationsToSeparateRelationships(relationshipBuilder);
                if (relationshipBuilder == null)
                {
                    return null;
                }

                fkPropertiesOnPrincipalToDependent = null;
            }

            var fkPropertiesOnNavigation = fkPropertiesOnDependentToPrincipal ?? fkPropertiesOnPrincipalToDependent;
            var upgradePrincipalToDependentNavigationSource = fkPropertiesOnPrincipalToDependent != null;
            var upgradeDependentToPrincipalNavigationSource = fkPropertiesOnDependentToPrincipal != null;
            ConfigurationSource? invertConfigurationSource = null;
            IReadOnlyList<string> fkPropertiesToSet;

            if (fkPropertiesOnNavigation == null
                || fkPropertiesOnNavigation.Count == 0)
            {
                if (fkPropertyOnDependent == null
                    && fkPropertyOnPrincipal == null)
                {
                    return relationshipBuilder;
                }

                if (fkPropertyOnDependent != null)
                {
                    fkPropertiesToSet = new List<string>
                    {
                        fkPropertyOnDependent.GetSimpleMemberName()
                    };
                    upgradeDependentToPrincipalNavigationSource = true;
                }
                else
                {
                    if (foreignKey.PrincipalToDependent.IsCollection())
                    {
                        return null;
                    }
                    invertConfigurationSource = ConfigurationSource.DataAnnotation;
                    fkPropertiesToSet = new List<string>
                    {
                        fkPropertyOnPrincipal.GetSimpleMemberName()
                    };
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
                        invertConfigurationSource = ConfigurationSource.DataAnnotation;
                    }
                }
                else
                {
                    var fkProperty = fkPropertyOnDependent ?? fkPropertyOnPrincipal;
                    if (fkPropertiesOnNavigation.Count != 1
                        || !Equals(fkPropertiesOnNavigation.First(), fkProperty.GetSimpleMemberName()))
                    {
                        _logger.ConflictingForeignKeyAttributesOnNavigationAndPropertyWarning(
                            fkPropertiesOnDependentToPrincipal != null
                                ? relationshipBuilder.Metadata.DependentToPrincipal
                                : relationshipBuilder.Metadata.PrincipalToDependent,
                            fkProperty);

                        relationshipBuilder = SplitNavigationsToSeparateRelationships(relationshipBuilder);
                        if (relationshipBuilder == null)
                        {
                            return null;
                        }

                        fkPropertiesToSet = fkPropertiesOnDependentToPrincipal ?? new List<string>
                        {
                            fkPropertyOnDependent.GetSimpleMemberName()
                        };
                    }

                    if (fkPropertyOnDependent != null)
                    {
                        upgradeDependentToPrincipalNavigationSource = true;
                    }
                    else
                    {
                        invertConfigurationSource = ConfigurationSource.DataAnnotation;
                    }
                }
            }

            var newRelationshipBuilder = relationshipBuilder;

            if (upgradeDependentToPrincipalNavigationSource)
            {
                newRelationshipBuilder.Metadata.UpdateDependentToPrincipalConfigurationSource(ConfigurationSource.DataAnnotation);
            }

            if (upgradePrincipalToDependentNavigationSource)
            {
                newRelationshipBuilder.Metadata.UpdatePrincipalToDependentConfigurationSource(ConfigurationSource.DataAnnotation);
            }

            if (invertConfigurationSource != null)
            {
                newRelationshipBuilder = newRelationshipBuilder.RelatedEntityTypes(
                    foreignKey.DeclaringEntityType, foreignKey.PrincipalEntityType, invertConfigurationSource.Value);
            }
            else
            {
                var existingProperties = foreignKey.DeclaringEntityType.Builder.GetOrCreateProperties(fkPropertiesToSet, null);
                if (existingProperties != null)
                {
                    var conflictingFk = foreignKey.DeclaringEntityType.FindForeignKeys(existingProperties)
                        .FirstOrDefault(fk => fk != foreignKey
                            && fk.PrincipalEntityType == foreignKey.PrincipalEntityType
                            && fk.GetConfigurationSource() == ConfigurationSource.DataAnnotation
                            && fk.GetForeignKeyPropertiesConfigurationSource() == ConfigurationSource.DataAnnotation);
                    if (conflictingFk != null)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.ConflictingForeignKeyAttributes(
                                Property.Format(existingProperties),
                                foreignKey.DeclaringEntityType.DisplayName()));
                    }
                }
            }

            return newRelationshipBuilder?.HasForeignKey(fkPropertiesToSet, ConfigurationSource.DataAnnotation) ?? relationshipBuilder;
        }

        private static InternalRelationshipBuilder SplitNavigationsToSeparateRelationships(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = relationshipBuilder.Metadata;
            var dependentToPrincipalNavigationName = foreignKey.DependentToPrincipal?.Name;
            var principalToDepedentNavigationName = foreignKey.PrincipalToDependent?.Name;

            if (GetInversePropertyAttribute(foreignKey.PrincipalToDependent) != null
                || GetInversePropertyAttribute(foreignKey.DependentToPrincipal) != null)
            {
                // Relationship is joined by InversePropertyAttribute
                throw new InvalidOperationException(
                    CoreStrings.InvalidRelationshipUsingDataAnnotations(
                        dependentToPrincipalNavigationName,
                        foreignKey.DeclaringEntityType.DisplayName(),
                        principalToDepedentNavigationName,
                        foreignKey.PrincipalEntityType.DisplayName()));
            }

            relationshipBuilder = relationshipBuilder.PrincipalToDependent((string)null, ConfigurationSource.DataAnnotation);
            return relationshipBuilder == null
                ? null
                : foreignKey.PrincipalEntityType.Builder.Relationship(
                       foreignKey.DeclaringEntityType.Builder,
                       principalToDepedentNavigationName,
                       null,
                       ConfigurationSource.DataAnnotation) == null
                ? null
                : relationshipBuilder;
        }

        private static ForeignKeyAttribute GetForeignKeyAttribute(TypeBase entityType, string propertyName)
            => entityType.GetRuntimeProperties()?.Values
                .FirstOrDefault(
                    p => string.Equals(p.GetSimpleMemberName(), propertyName, StringComparison.OrdinalIgnoreCase)
                         && Attribute.IsDefined(p, typeof(ForeignKeyAttribute), inherit: true))
                ?.GetCustomAttribute<ForeignKeyAttribute>(inherit: true);

        private static ForeignKeyAttribute GetForeignKeyAttribute(Navigation navigation)
            => GetAttribute<ForeignKeyAttribute>(navigation.PropertyInfo);

        private static InversePropertyAttribute GetInversePropertyAttribute(Navigation navigation)
            => GetAttribute<InversePropertyAttribute>(navigation.PropertyInfo);

        private static TAttribute GetAttribute<TAttribute>(MemberInfo memberInfo)
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
        private MemberInfo FindForeignKeyAttributeOnProperty(EntityType entityType, string navigationName)
        {
            if (string.IsNullOrWhiteSpace(navigationName)
                || !entityType.HasClrType())
            {
                return null;
            }

            MemberInfo candidateProperty = null;

            foreach (var memberInfo in entityType.GetRuntimeProperties().Values.Cast<MemberInfo>()
                .Concat(entityType.GetRuntimeFields().Values))
            {
                if (entityType.Builder.IsIgnored(memberInfo.GetSimpleMemberName(), ConfigurationSource.Convention)
                    || !Attribute.IsDefined(memberInfo, typeof(ForeignKeyAttribute), inherit: true))
                {
                    continue;
                }

                var attribute = memberInfo.GetCustomAttribute<ForeignKeyAttribute>(inherit: true);

                if (attribute.Name != navigationName
                    || (memberInfo is PropertyInfo propertyInfo
                        && FindCandidateNavigationPropertyType(propertyInfo) != null))
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
            => _memberClassifier.FindCandidateNavigationPropertyType(propertyInfo);

        private static IReadOnlyList<string> FindCandidateDependentPropertiesThroughNavigation(
            InternalRelationshipBuilder relationshipBuilder,
            bool pointsToPrincipal)
        {
            var navigation = pointsToPrincipal
                ? relationshipBuilder.Metadata.DependentToPrincipal
                : relationshipBuilder.Metadata.PrincipalToDependent;

            var navigationFkAttribute = navigation != null
                ? GetForeignKeyAttribute(navigation)
                : null;

            if (navigationFkAttribute != null)
            {
                var properties = navigationFkAttribute.Name.Split(',').Select(p => p.Trim()).ToList();

                if (properties.Any(string.IsNullOrWhiteSpace))
                {
                    throw new InvalidOperationException(
                        CoreStrings.InvalidPropertyListOnNavigation(navigation.Name, navigation.DeclaringEntityType.DisplayName()));
                }

                var navigationPropertyTargetType =
                    navigation.DeclaringEntityType.GetRuntimeProperties()[navigation.Name].PropertyType;

                var otherNavigations = navigation.DeclaringEntityType.GetRuntimeProperties().Values
                    .Where(p => p.PropertyType == navigationPropertyTargetType && p.GetSimpleMemberName() != navigation.Name)
                    .OrderBy(p => p.GetSimpleMemberName());

                foreach (var propertyInfo in otherNavigations)
                {
                    var attribute = GetAttribute<ForeignKeyAttribute>(propertyInfo);
                    if (attribute?.Name == navigationFkAttribute.Name)
                    {
                        throw new InvalidOperationException(
                            CoreStrings.MultipleNavigationsSameFk(navigation.DeclaringEntityType.DisplayName(), attribute.Name));
                    }
                }

                return properties;
            }

            return null;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
            {
                foreach (var declaredNavigation in entityType.GetDeclaredNavigations())
                {
                    if (declaredNavigation.IsCollection())
                    {
                        var foreignKey = declaredNavigation.ForeignKey;
                        var fkPropertyOnPrincipal
                            = FindForeignKeyAttributeOnProperty(foreignKey.PrincipalEntityType, declaredNavigation.Name);
                        if (fkPropertyOnPrincipal != null)
                        {
                            throw new InvalidOperationException(CoreStrings.FkAttributeOnNonUniquePrincipal(
                                declaredNavigation.Name,
                                foreignKey.PrincipalEntityType.DisplayName(),
                                foreignKey.DeclaringEntityType.DisplayName()));
                        }
                    }
                }
            }

            return modelBuilder;
        }
    }
}
