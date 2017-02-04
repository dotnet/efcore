// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InversePropertyAttributeConvention : NavigationAttributeEntityTypeConvention<InversePropertyAttribute>
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public const string InverseNavigationsAnnotationName = "InversePropertyAttributeConvention:InverseNavigations";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalEntityTypeBuilder Apply(
            InternalEntityTypeBuilder entityTypeBuilder,
            PropertyInfo navigationPropertyInfo,
            Type targetClrType,
            InversePropertyAttribute attribute)
        {
            Check.NotNull(entityTypeBuilder, nameof(entityTypeBuilder));
            Check.NotNull(navigationPropertyInfo, nameof(navigationPropertyInfo));
            Check.NotNull(attribute, nameof(attribute));

            if (entityTypeBuilder.Metadata.HasDelegatedIdentity()
                || entityTypeBuilder.ModelBuilder.Metadata.IsDelegatedIdentityEntityType(targetClrType))
            {
                return entityTypeBuilder;
            }

            var targetEntityTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(targetClrType, ConfigurationSource.DataAnnotation);
            if (targetEntityTypeBuilder == null)
            {
                return entityTypeBuilder;
            }

            if (!entityTypeBuilder.CanAddOrReplaceNavigation(navigationPropertyInfo.Name, ConfigurationSource.DataAnnotation))
            {
                return entityTypeBuilder;
            }

            ConfigureInverseNavigation(entityTypeBuilder, navigationPropertyInfo, targetEntityTypeBuilder, attribute);

            return entityTypeBuilder;
        }

        private InternalRelationshipBuilder ConfigureInverseNavigation(
            InternalEntityTypeBuilder entityTypeBuilder,
            PropertyInfo navigationPropertyInfo,
            InternalEntityTypeBuilder targetEntityTypeBuilder,
            InversePropertyAttribute attribute)
        {
            var entityType = entityTypeBuilder.Metadata;
            var targetClrType = targetEntityTypeBuilder.Metadata.ClrType;
            var inverseNavigationPropertyInfo = targetClrType.GetRuntimeProperties().FirstOrDefault(p => string.Equals(p.Name, attribute.Property, StringComparison.OrdinalIgnoreCase));

            if ((inverseNavigationPropertyInfo == null)
                || !FindCandidateNavigationPropertyType(inverseNavigationPropertyInfo).GetTypeInfo()
                    .IsAssignableFrom(entityType.ClrType.GetTypeInfo()))
            {
                throw new InvalidOperationException(
                    CoreStrings.InvalidNavigationWithInverseProperty(
                        navigationPropertyInfo.Name, entityType.DisplayName(), attribute.Property, targetClrType.ShortDisplayName()));
            }

            if (Equals(inverseNavigationPropertyInfo, navigationPropertyInfo))
            {
                throw new InvalidOperationException(
                    CoreStrings.SelfReferencingNavigationWithInverseProperty(
                        navigationPropertyInfo.Name,
                        entityType.DisplayName(),
                        navigationPropertyInfo.Name,
                        entityType.DisplayName()));
            }

            // Check for InversePropertyAttribute on the inverseNavigation to verify that it matches.
            var inverseAttribute = inverseNavigationPropertyInfo.GetCustomAttribute<InversePropertyAttribute>(true);
            if (inverseAttribute != null
                && inverseAttribute.Property != navigationPropertyInfo.Name)
            {
                throw new InvalidOperationException(
                    CoreStrings.InversePropertyMismatch(
                        navigationPropertyInfo.Name,
                        entityType.DisplayName(),
                        inverseNavigationPropertyInfo.Name,
                        targetEntityTypeBuilder.Metadata.DisplayName()));
            }

            var referencingNavigationsWithAttribute =
                AddInverseNavigation(entityType, navigationPropertyInfo, targetEntityTypeBuilder.Metadata, inverseNavigationPropertyInfo);

            if (IsAmbiguousInverse(entityType, navigationPropertyInfo, referencingNavigationsWithAttribute))
            {
                var existingInverse = targetEntityTypeBuilder.Metadata.FindNavigation(inverseNavigationPropertyInfo)?.FindInverse();
                if (existingInverse != null
                    && IsAmbiguousInverse(existingInverse.DeclaringEntityType, existingInverse.PropertyInfo, referencingNavigationsWithAttribute))
                {
                    var fk = existingInverse.ForeignKey;
                    if (fk.GetConfigurationSource() == ConfigurationSource.DataAnnotation)
                    {
                        fk.DeclaringEntityType.Builder.RemoveForeignKey(fk, ConfigurationSource.DataAnnotation);
                    }
                }

                return entityTypeBuilder.Metadata.FindNavigation(navigationPropertyInfo)?.ForeignKey.Builder;
            }

            return targetEntityTypeBuilder.Relationship(
                entityTypeBuilder,
                inverseNavigationPropertyInfo,
                navigationPropertyInfo,
                ConfigurationSource.DataAnnotation);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool Apply(
            InternalModelBuilder modelBuilder,
            Type type,
            PropertyInfo navigationPropertyInfo,
            Type targetClrType,
            InversePropertyAttribute attribute)
        {
            var declaringType = navigationPropertyInfo.DeclaringType;
            Debug.Assert(declaringType != null);
            if (modelBuilder.Metadata.FindEntityType(declaringType) != null)
            {
                return true;
            }

            var leastDerivedEntityTypes = modelBuilder.FindLeastDerivedEntityTypes(declaringType,
                t => !t.IsIgnored(navigationPropertyInfo.Name, ConfigurationSource.DataAnnotation));
            foreach (var leastDerivedEntityType in leastDerivedEntityTypes)
            {
                Apply(leastDerivedEntityType, navigationPropertyInfo, targetClrType, attribute);
            }
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalRelationshipBuilder Apply(
            InternalRelationshipBuilder relationshipBuilder, Navigation navigation, InversePropertyAttribute attribute)
        {
            if (relationshipBuilder.Metadata.DeclaringEntityType.HasDelegatedIdentity()
                || relationshipBuilder.Metadata.PrincipalEntityType.HasDelegatedIdentity())
            {
                return relationshipBuilder;
            }

            return ConfigureInverseNavigation(
                navigation.DeclaringEntityType.Builder, navigation.PropertyInfo, navigation.GetTargetType().Builder, attribute);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool Apply(
            InternalEntityTypeBuilder entityTypeBuilder,
            EntityType oldBaseType,
            PropertyInfo navigationPropertyInfo,
            Type targetClrType,
            InversePropertyAttribute attribute)
        {
            var entityClrType = entityTypeBuilder.Metadata.ClrType;
            if (navigationPropertyInfo.DeclaringType != entityClrType)
            {
                var newBaseType = entityTypeBuilder.Metadata.BaseType;
                if (newBaseType == null)
                {
                    Apply(entityTypeBuilder, navigationPropertyInfo, targetClrType, attribute);
                }
                else
                {
                    var targetEntityType = entityTypeBuilder.Metadata.Model.FindEntityType(targetClrType);
                    if (targetEntityType == null)
                    {
                        return true;
                    }

                    RemoveInverseNavigation(entityTypeBuilder.Metadata, navigationPropertyInfo, targetEntityType);
                }
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override bool ApplyIgnored(
            InternalEntityTypeBuilder entityTypeBuilder,
            PropertyInfo navigationPropertyInfo,
            Type targetClrType,
            InversePropertyAttribute attribute)
        {
            var targetEntityType = entityTypeBuilder.Metadata.Model.FindEntityType(targetClrType);
            if (targetEntityType == null)
            {
                return true;
            }

            RemoveInverseNavigation(entityTypeBuilder.Metadata, navigationPropertyInfo, targetEntityType);
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsAmbiguous(
            [NotNull] EntityType entityType, [NotNull] PropertyInfo navigation, [NotNull] EntityType targetEntityType)
        {
            var inverseNavigations = GetInverseNavigations(targetEntityType);
            if (inverseNavigations == null)
            {
                return false;
            }

            foreach (var inverseNavigationPair in inverseNavigations)
            {
                if (IsAmbiguousInverse(entityType, navigation, inverseNavigationPair.Value))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAmbiguousInverse(
            EntityType entityType, PropertyInfo navigation, List<Tuple<PropertyInfo, Type>> referencingNavigationsWithAttribute)
        {
            if (referencingNavigationsWithAttribute.Count == 1)
            {
                return false;
            }

            foreach (var referencingTuple in referencingNavigationsWithAttribute)
            {
                var inverseTargetEntityType = entityType.Model.FindEntityType(referencingTuple.Item2);

                var isInverseDelegated = entityType.Model.IsDelegatedIdentityEntityType(referencingTuple.Item2);
                if ((inverseTargetEntityType == null
                     || inverseTargetEntityType.Builder.IsIgnored(referencingTuple.Item1.Name, ConfigurationSource.DataAnnotation))
                    && !isInverseDelegated)
                {
                    continue;
                }

                if (referencingTuple.Item1.Name != navigation.Name)
                {
                    return true;
                }

                if ((isInverseDelegated && entityType.ClrType != referencingTuple.Item2)
                    || (!isInverseDelegated && !entityType.IsSameHierarchy(inverseTargetEntityType)))
                {
                    return true;
                }
            }

            return false;
        }

        private List<Tuple<PropertyInfo, Type>> AddInverseNavigation(
            EntityType entityType, PropertyInfo navigation, EntityType targetEntityType, PropertyInfo inverseNavigation)
        {
            var inverseNavigations = GetInverseNavigations(targetEntityType);
            if (inverseNavigations == null)
            {
                inverseNavigations = new Dictionary<PropertyInfo, List<Tuple<PropertyInfo, Type>>>();
                SetInverseNavigations(targetEntityType.Builder, inverseNavigations);
            }

            if (!inverseNavigations.TryGetValue(inverseNavigation, out List<Tuple<PropertyInfo, Type>> referencingNavigationsWithAttribute))
            {
                referencingNavigationsWithAttribute = new List<Tuple<PropertyInfo, Type>>();
                inverseNavigations[inverseNavigation] = referencingNavigationsWithAttribute;
            }

            foreach (var referencingTuple in referencingNavigationsWithAttribute)
            {
                if (referencingTuple.Item1.IsSameAs(navigation)
                    && referencingTuple.Item2 == entityType.ClrType)
                {
                    return referencingNavigationsWithAttribute;
                }
            }

            referencingNavigationsWithAttribute.Add(Tuple.Create(navigation, entityType.ClrType));

            return referencingNavigationsWithAttribute;
        }

        private static bool RemoveInverseNavigation(
            EntityType entityType, PropertyInfo navigation, EntityType targetEntityType)
        {
            var inverseNavigations = GetInverseNavigations(targetEntityType);
            if (inverseNavigations == null)
            {
                return false;
            }

            foreach (var inverseNavigationPair in inverseNavigations)
            {
                var inverseNavigation = inverseNavigationPair.Key;
                var referencingNavigationsWithAttribute = inverseNavigationPair.Value;

                for (var index = 0; index < referencingNavigationsWithAttribute.Count; index++)
                {
                    var referencingTuple = referencingNavigationsWithAttribute[index];
                    if (referencingTuple.Item1.IsSameAs(navigation)
                        && referencingTuple.Item2 == entityType.ClrType)
                    {
                        referencingNavigationsWithAttribute.RemoveAt(index);
                        if (!referencingNavigationsWithAttribute.Any())
                        {
                            inverseNavigations.Remove(inverseNavigation);
                        }
                        if (referencingNavigationsWithAttribute.Count == 1)
                        {
                            var clrType = referencingNavigationsWithAttribute[0].Item2;
                            if (!entityType.Model.IsDelegatedIdentityEntityType(clrType))
                            {
                                targetEntityType.Builder.Relationship(
                                    entityType.Model.Builder.Entity(clrType, ConfigurationSource.DataAnnotation),
                                    inverseNavigation,
                                    referencingNavigationsWithAttribute[0].Item1,
                                    ConfigurationSource.DataAnnotation);
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private static Dictionary<PropertyInfo, List<Tuple<PropertyInfo, Type>>> GetInverseNavigations(EntityType entityType)
            => entityType.FindAnnotation(InverseNavigationsAnnotationName)?.Value
                as Dictionary<PropertyInfo, List<Tuple<PropertyInfo, Type>>>;

        private void SetInverseNavigations(
            InternalEntityTypeBuilder entityTypeBuilder,
            Dictionary<PropertyInfo, List<Tuple<PropertyInfo, Type>>> inverseNavigations)
            => entityTypeBuilder.HasAnnotation(InverseNavigationsAnnotationName, inverseNavigations, ConfigurationSource.Convention);
    }
}
