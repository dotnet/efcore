// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

            if (inverseNavigationPropertyInfo == navigationPropertyInfo)
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

            var inverseNavigationsList = AddInverseNavigation(
                targetEntityTypeBuilder.Metadata, inverseNavigationPropertyInfo, entityType.ClrType, navigationPropertyInfo);

            if (IsAmbiguousInverse(navigationPropertyInfo, entityType, inverseNavigationsList))
            {
                var existingInverse = targetEntityTypeBuilder.Metadata.FindNavigation(inverseNavigationPropertyInfo)?.FindInverse();

                if (existingInverse != null)
                {
                    Debug.Assert(!existingInverse.DeclaringEntityType.IsAssignableFrom(entityType)
                                 == IsAmbiguousInverse(
                                     existingInverse.PropertyInfo,
                                     existingInverse.DeclaringEntityType,
                                     inverseNavigationsList));
                }

                if (existingInverse != null
                    && IsAmbiguousInverse(
                        existingInverse.PropertyInfo,
                        existingInverse.DeclaringEntityType,
                        inverseNavigationsList))
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
            if (leastDerivedEntityTypes.Count != 1)
            {
                return true;
            }

            Apply(leastDerivedEntityTypes.Single(), navigationPropertyInfo, targetClrType, attribute);
            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override InternalRelationshipBuilder Apply(
            InternalRelationshipBuilder relationshipBuilder, Navigation navigation, InversePropertyAttribute attribute)
            => ConfigureInverseNavigation(
                navigation.DeclaringEntityType.Builder, navigation.PropertyInfo, navigation.GetTargetType().Builder, attribute);

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
            if (navigationPropertyInfo.DeclaringType != entityTypeBuilder.Metadata.ClrType)
            {
                var newBaseType = entityTypeBuilder.Metadata.BaseType;
                if (newBaseType == null)
                {
                    Apply(entityTypeBuilder, navigationPropertyInfo, targetClrType, attribute);
                }
                else
                {
                    Apply(
                        newBaseType.Builder,
                        newBaseType.ClrType.GetRuntimeProperties().First(p => p.Name == navigationPropertyInfo.Name),
                        targetClrType,
                        attribute);
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
            var entityType = entityTypeBuilder.Metadata;
            var targetType = entityType.Model.FindEntityType(targetClrType);
            var inverseNavigationPropertyInfo = targetClrType.GetRuntimeProperties()
                .FirstOrDefault(p => string.Equals(p.Name, attribute.Property, StringComparison.OrdinalIgnoreCase));
            if (targetType == null
                || inverseNavigationPropertyInfo == null)
            {
                return true;
            }

            List<Tuple<PropertyInfo, Type>> navigationTuples;
            var inverseNavigations = GetInverseNavigations(targetType);
            if (inverseNavigations == null
                || !inverseNavigations.TryGetValue(inverseNavigationPropertyInfo, out navigationTuples))
            {
                return true;
            }

            var inverseWasAmbiguous = false;
            for (var index = 0; index < navigationTuples.Count; index++)
            {
                var inverseTuple = navigationTuples[index];
                if (inverseTuple.Item1 == navigationPropertyInfo
                    && inverseTuple.Item2 == entityType.ClrType)
                {
                    navigationTuples.RemoveAt(index);
                    if (!navigationTuples.Any())
                    {
                        inverseNavigations.Remove(inverseNavigationPropertyInfo);
                    }
                    inverseWasAmbiguous = true;
                    break;
                }
            }

            if (!inverseWasAmbiguous
                || navigationTuples.Count > 1)
            {
                return true;
            }

            var otherEntityTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(navigationTuples[0].Item2, ConfigurationSource.DataAnnotation);
            targetType.Builder.Relationship(
                otherEntityTypeBuilder,
                inverseNavigationPropertyInfo,
                navigationTuples[0].Item1,
                ConfigurationSource.DataAnnotation);

            return true;
        }

        private static bool IsAmbiguousInverse(
            PropertyInfo navigationPropertyInfo, EntityType entityType, List<Tuple<PropertyInfo, Type>> inverseNavigationsList)
        {
            foreach (var inverseNavigationTuple in inverseNavigationsList)
            {
                var inverseTargetEntityType = entityType.Model.FindEntityType(inverseNavigationTuple.Item2);
                if (inverseTargetEntityType == null
                    || inverseTargetEntityType.Builder.IsIgnored(inverseNavigationTuple.Item1.Name, ConfigurationSource.DataAnnotation))
                {
                    continue;
                }

                if (inverseNavigationTuple.Item1.Name != navigationPropertyInfo.Name)
                {
                    return true;
                }

                if (!entityType.IsAssignableFrom(inverseTargetEntityType))
                {
                    return true;
                }
            }

            return false;
        }

        private List<Tuple<PropertyInfo, Type>> AddInverseNavigation(
            EntityType entityType, PropertyInfo navigation, Type inverseEntityClrType, PropertyInfo inverseNavigation)
        {
            var referencingNavigationsWithAttribute = GetInverseNavigations(entityType);
            if (referencingNavigationsWithAttribute == null)
            {
                referencingNavigationsWithAttribute = new Dictionary<PropertyInfo, List<Tuple<PropertyInfo, Type>>>();
                SetInverseNavigations(entityType.Builder, referencingNavigationsWithAttribute);
            }

            List<Tuple<PropertyInfo, Type>> inverseNavigationsList;
            if (!referencingNavigationsWithAttribute.TryGetValue(navigation, out inverseNavigationsList))
            {
                inverseNavigationsList = new List<Tuple<PropertyInfo, Type>>();
                referencingNavigationsWithAttribute[navigation] = inverseNavigationsList;
            }

            foreach (var inverseTuple in inverseNavigationsList)
            {
                if (inverseTuple.Item1 == inverseNavigation
                    && inverseTuple.Item2 == inverseEntityClrType)
                {
                    return inverseNavigationsList;
                }
            }

            inverseNavigationsList.Add(Tuple.Create(inverseNavigation, inverseEntityClrType));

            return inverseNavigationsList;
        }

        private Dictionary<PropertyInfo, List<Tuple<PropertyInfo, Type>>> GetInverseNavigations(EntityType entityType)
            => entityType.FindAnnotation(InverseNavigationsAnnotationName)?.Value
                as Dictionary<PropertyInfo, List<Tuple<PropertyInfo, Type>>>;

        private void SetInverseNavigations(
            InternalEntityTypeBuilder entityTypeBuilder,
            Dictionary<PropertyInfo, List<Tuple<PropertyInfo, Type>>> inverseNavigations)
            => entityTypeBuilder.HasAnnotation(InverseNavigationsAnnotationName, inverseNavigations, ConfigurationSource.Convention);
    }
}
