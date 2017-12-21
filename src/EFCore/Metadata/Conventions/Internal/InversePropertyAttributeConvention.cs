// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InversePropertyAttributeConvention :
        NavigationAttributeEntityTypeConvention<InversePropertyAttribute>, IModelBuiltConvention
    {
        private readonly IDiagnosticsLogger<DbLoggerCategory.Model> _logger;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InversePropertyAttributeConvention(
            [NotNull] ITypeMapper typeMapper, [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
            : base(typeMapper)
        {
            _logger = logger;
        }

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

            if (inverseNavigationPropertyInfo == null
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

            if (IsAmbiguousInverse(navigationPropertyInfo, entityType.ClrType, entityType.Model, referencingNavigationsWithAttribute))
            {
                var existingInverse = targetEntityTypeBuilder.Metadata.FindNavigation(inverseNavigationPropertyInfo)?.FindInverse();
                var existingInverseType = existingInverse?.DeclaringEntityType.ClrType;
                if (existingInverse != null
                    && IsAmbiguousInverse(
                        existingInverse.MemberInfo, existingInverseType, entityType.Model, referencingNavigationsWithAttribute))
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

            var leastDerivedEntityTypes = modelBuilder.FindLeastDerivedEntityTypes(
                declaringType,
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
            if (relationshipBuilder.Metadata.DeclaringEntityType.HasDefiningNavigation()
                || relationshipBuilder.Metadata.PrincipalEntityType.HasDefiningNavigation())
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
        public virtual InternalModelBuilder Apply(InternalModelBuilder modelBuilder)
        {
            var model = modelBuilder.Metadata;
            foreach (var entityType in model.GetEntityTypes())
            {
                var inverseNavigations = GetInverseNavigations(entityType);
                if (inverseNavigations == null)
                {
                    continue;
                }

                foreach (var inverseNavigation in inverseNavigations)
                {
                    foreach (var referencingNavigationWithAttribute in inverseNavigation.Value)
                    {
                        var ambiguousInverse = FindAmbiguousInverse(
                            referencingNavigationWithAttribute.Item1,
                            referencingNavigationWithAttribute.Item2,
                            model,
                            inverseNavigation.Value);
                        if (ambiguousInverse != null)
                        {
                            _logger.MultipleInversePropertiesSameTarget(
                                new[] { referencingNavigationWithAttribute, ambiguousInverse },
                                inverseNavigation.Key,
                                entityType.ClrType);
                            break;
                        }
                    }
                }
            }

            return modelBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static bool IsAmbiguous(
            [NotNull] EntityType entityType, [NotNull] MemberInfo navigation, [NotNull] EntityType targetEntityType)
        {
            var inverseNavigations = GetInverseNavigations(targetEntityType);
            if (inverseNavigations == null)
            {
                return false;
            }

            foreach (var inverseNavigation in inverseNavigations)
            {
                if (IsAmbiguousInverse(navigation, entityType.ClrType, entityType.Model, inverseNavigation.Value))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAmbiguousInverse(
            MemberInfo navigation,
            Type entityType,
            Model model,
            List<Tuple<MemberInfo, Type>> referencingNavigationsWithAttribute)
            => FindAmbiguousInverse(navigation, entityType, model, referencingNavigationsWithAttribute) != null;

        private static Tuple<MemberInfo, Type> FindAmbiguousInverse(
            MemberInfo navigation,
            Type entityType,
            Model model,
            List<Tuple<MemberInfo, Type>> referencingNavigationsWithAttribute)
        {
            if (referencingNavigationsWithAttribute.Count == 1)
            {
                return null;
            }

            List<Tuple<MemberInfo, Type>> tuplesToRemove = null;
            Tuple<MemberInfo, Type> ambiguousTuple = null;
            foreach (var referencingTuple in referencingNavigationsWithAttribute)
            {
                var inverseTargetEntityType = model.FindEntityType(referencingTuple.Item2);
                var isInverseDependent = model.HasEntityTypeWithDefiningNavigation(referencingTuple.Item2);
                if ((inverseTargetEntityType == null
                     || inverseTargetEntityType.Builder.IsIgnored(referencingTuple.Item1.Name, ConfigurationSource.DataAnnotation))
                    && !isInverseDependent)
                {
                    if (tuplesToRemove == null)
                    {
                        tuplesToRemove = new List<Tuple<MemberInfo, Type>>();
                    }
                    tuplesToRemove.Add(referencingTuple);
                    continue;
                }

                if (referencingTuple.Item1.Name != navigation.Name)
                {
                    ambiguousTuple = referencingTuple;
                    break;
                }

                if (isInverseDependent)
                {
                    if (entityType != referencingTuple.Item2)
                    {
                        ambiguousTuple = referencingTuple;
                        break;
                    }
                }
                else
                {
                    var sourceEntityType = model.FindEntityType(entityType);
                    if (sourceEntityType != null
                        && !sourceEntityType.IsSameHierarchy(inverseTargetEntityType))
                    {
                        ambiguousTuple = referencingTuple;
                        break;
                    }
                }
            }

            if (tuplesToRemove != null)
            {
                foreach (var tuple in tuplesToRemove)
                {
                    referencingNavigationsWithAttribute.Remove(tuple);
                }
            }

            return ambiguousTuple;
        }

        private static List<Tuple<MemberInfo, Type>> AddInverseNavigation(
            TypeBase entityType, MemberInfo navigation, EntityType targetEntityType, MemberInfo inverseNavigation)
        {
            var inverseNavigations = GetInverseNavigations(targetEntityType);
            if (inverseNavigations == null)
            {
                inverseNavigations = new Dictionary<MemberInfo, List<Tuple<MemberInfo, Type>>>();
                SetInverseNavigations(targetEntityType.Builder, inverseNavigations);
            }

            if (!inverseNavigations.TryGetValue(inverseNavigation, out var referencingNavigationsWithAttribute))
            {
                referencingNavigationsWithAttribute = new List<Tuple<MemberInfo, Type>>();
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

        private static void RemoveInverseNavigation(
            TypeBase entityType,
            MemberInfo navigation,
            EntityType targetEntityType)
        {
            var inverseNavigations = GetInverseNavigations(targetEntityType);

            if (inverseNavigations == null)
            {
                return;
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
                            if (!entityType.Model.HasEntityTypeWithDefiningNavigation(clrType))
                            {
                                targetEntityType.Builder.Relationship(
                                    entityType.Model.Builder.Entity(clrType, ConfigurationSource.DataAnnotation),
                                    (PropertyInfo)inverseNavigation,
                                    (PropertyInfo)referencingNavigationsWithAttribute[0].Item1,
                                    ConfigurationSource.DataAnnotation);
                            }
                        }

                        return;
                    }
                }
            }
        }

        private static Dictionary<MemberInfo, List<Tuple<MemberInfo, Type>>> GetInverseNavigations(
            ConventionalAnnotatable entityType)
            => entityType.FindAnnotation(InverseNavigationsAnnotationName)?.Value
                as Dictionary<MemberInfo, List<Tuple<MemberInfo, Type>>>;

        private static void SetInverseNavigations(
            InternalMetadataBuilder entityTypeBuilder,
            Dictionary<MemberInfo, List<Tuple<MemberInfo, Type>>> inverseNavigations)
            => entityTypeBuilder.HasAnnotation(InverseNavigationsAnnotationName, inverseNavigations, ConfigurationSource.Convention);
    }
}
