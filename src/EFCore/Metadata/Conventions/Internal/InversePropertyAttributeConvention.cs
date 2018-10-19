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
            [NotNull] IMemberClassifier memberClassifier,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
            : base(memberClassifier)
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

            if (!entityTypeBuilder.CanAddOrReplaceNavigation(navigationPropertyInfo.GetSimpleMemberName(), ConfigurationSource.DataAnnotation))
            {
                return entityTypeBuilder;
            }

            var targetEntityTypeBuilder = RelationshipDiscoveryConvention.GetTargetEntityTypeBuilder(
                entityTypeBuilder, targetClrType, navigationPropertyInfo, ConfigurationSource.DataAnnotation);

            if (targetEntityTypeBuilder == null)
            {
                return entityTypeBuilder;
            }

            ConfigureInverseNavigation(entityTypeBuilder, navigationPropertyInfo, targetEntityTypeBuilder, attribute);

            return entityTypeBuilder;
        }

        private InternalRelationshipBuilder ConfigureInverseNavigation(
            InternalEntityTypeBuilder entityTypeBuilder,
            MemberInfo navigationMemberInfo,
            InternalEntityTypeBuilder targetEntityTypeBuilder,
            InversePropertyAttribute attribute)
        {
            var entityType = entityTypeBuilder.Metadata;
            var targetClrType = targetEntityTypeBuilder.Metadata.ClrType;
            var inverseNavigationPropertyInfo = targetEntityTypeBuilder.Metadata.GetRuntimeProperties().Values
                .FirstOrDefault(p => string.Equals(p.GetSimpleMemberName(), attribute.Property, StringComparison.OrdinalIgnoreCase));

            if (inverseNavigationPropertyInfo == null
                || !FindCandidateNavigationPropertyType(inverseNavigationPropertyInfo).GetTypeInfo()
                    .IsAssignableFrom(entityType.ClrType.GetTypeInfo()))
            {
                throw new InvalidOperationException(
                    CoreStrings.InvalidNavigationWithInverseProperty(
                        navigationMemberInfo.Name, entityType.DisplayName(), attribute.Property, targetClrType.ShortDisplayName()));
            }

            if (Equals(inverseNavigationPropertyInfo, navigationMemberInfo))
            {
                throw new InvalidOperationException(
                    CoreStrings.SelfReferencingNavigationWithInverseProperty(
                        navigationMemberInfo.Name,
                        entityType.DisplayName(),
                        navigationMemberInfo.Name,
                        entityType.DisplayName()));
            }

            // Check for InversePropertyAttribute on the inverseNavigation to verify that it matches.
            if (Attribute.IsDefined(inverseNavigationPropertyInfo, typeof(InversePropertyAttribute)))
            {
                var inverseAttribute = inverseNavigationPropertyInfo.GetCustomAttribute<InversePropertyAttribute>(true);
                if (inverseAttribute.Property != navigationMemberInfo.GetSimpleMemberName())
                {
                    throw new InvalidOperationException(
                        CoreStrings.InversePropertyMismatch(
                            navigationMemberInfo.Name,
                            entityType.DisplayName(),
                            inverseNavigationPropertyInfo.Name,
                            targetEntityTypeBuilder.Metadata.DisplayName()));
                }
            }

            var referencingNavigationsWithAttribute =
                AddInverseNavigation(entityType, navigationMemberInfo, targetEntityTypeBuilder.Metadata, inverseNavigationPropertyInfo);

            var ambiguousInverse = FindAmbiguousInverse(
                navigationMemberInfo, entityType, entityType.Model, referencingNavigationsWithAttribute);
            if (ambiguousInverse != null)
            {
                var existingInverse = targetEntityTypeBuilder.Metadata.FindNavigation(inverseNavigationPropertyInfo)?.FindInverse();
                var existingInverseType = existingInverse?.DeclaringEntityType;
                if (existingInverse != null
                    && IsAmbiguousInverse(
                        existingInverse.GetIdentifyingMemberInfo(), existingInverseType, entityType.Model, referencingNavigationsWithAttribute))
                {
                    var fk = existingInverse.ForeignKey;
                    if (fk.IsOwnership
                        || fk.DeclaringEntityType.Builder.RemoveForeignKey(fk, ConfigurationSource.DataAnnotation) == null)
                    {
                        fk.Builder.Navigations(
                            existingInverse.IsDependentToPrincipal() ? PropertyIdentity.None : (PropertyIdentity?)null,
                            existingInverse.IsDependentToPrincipal() ? (PropertyIdentity?)null : PropertyIdentity.None,
                            ConfigurationSource.DataAnnotation);
                    }
                }

                var existingNavigation = entityTypeBuilder.Metadata.FindNavigation(navigationMemberInfo);
                if (existingNavigation != null)
                {
                    var fk = existingNavigation.ForeignKey;
                    if (fk.IsOwnership
                        || fk.DeclaringEntityType.Builder.RemoveForeignKey(fk, ConfigurationSource.DataAnnotation) == null)
                    {
                        fk.Builder.Navigations(
                            existingNavigation.IsDependentToPrincipal() ? PropertyIdentity.None : (PropertyIdentity?)null,
                            existingNavigation.IsDependentToPrincipal() ? (PropertyIdentity?)null : PropertyIdentity.None,
                            ConfigurationSource.DataAnnotation);
                    }
                }

                var existingAmbiguousNavigation = entityType.Model.FindActualEntityType(ambiguousInverse.Value.Item2)
                    .FindNavigation(ambiguousInverse.Value.Item1);
                if (existingAmbiguousNavigation != null)
                {
                    var fk = existingAmbiguousNavigation.ForeignKey;
                    if (fk.IsOwnership
                        || fk.DeclaringEntityType.Builder.RemoveForeignKey(fk, ConfigurationSource.DataAnnotation) == null)
                    {
                        fk.Builder.Navigations(
                            existingAmbiguousNavigation.IsDependentToPrincipal() ? PropertyIdentity.None : (PropertyIdentity?)null,
                            existingAmbiguousNavigation.IsDependentToPrincipal() ? (PropertyIdentity?)null : PropertyIdentity.None,
                            ConfigurationSource.DataAnnotation);
                    }
                }

                return entityTypeBuilder.Metadata.FindNavigation(navigationMemberInfo)?.ForeignKey.Builder;
            }

            var ownership = entityType.FindOwnership();
            if (ownership != null
                && ownership.PrincipalEntityType == targetEntityTypeBuilder.Metadata
                && ownership.PrincipalToDependent?.GetIdentifyingMemberInfo() != inverseNavigationPropertyInfo)
            {
                _logger.NonOwnershipInverseNavigationWarning(
                    entityType, navigationMemberInfo,
                    targetEntityTypeBuilder.Metadata, inverseNavigationPropertyInfo,
                    ownership.PrincipalToDependent.GetIdentifyingMemberInfo());
                return null;
            }

            if (entityType.DefiningEntityType != null
                && entityType.DefiningEntityType == targetEntityTypeBuilder.Metadata
                && entityType.DefiningNavigationName != inverseNavigationPropertyInfo.GetSimpleMemberName())
            {
                _logger.NonDefiningInverseNavigationWarning(
                    entityType, navigationMemberInfo,
                    targetEntityTypeBuilder.Metadata, inverseNavigationPropertyInfo,
                    entityType.DefiningEntityType.GetRuntimeProperties()[entityType.DefiningNavigationName]);
                return null;
            }

            return entityType.Model.ShouldBeOwnedType(entityType.ClrType)
                && !entityType.IsInOwnershipPath(targetEntityTypeBuilder.Metadata)
                ? targetEntityTypeBuilder.Owns(
                    entityTypeBuilder.Metadata.ClrType,
                    inverseNavigationPropertyInfo,
                    navigationMemberInfo,
                    ConfigurationSource.Convention)
                : targetEntityTypeBuilder.Relationship(
                entityTypeBuilder,
                inverseNavigationPropertyInfo,
                navigationMemberInfo,
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
                t => !t.IsIgnored(navigationPropertyInfo.GetSimpleMemberName(), ConfigurationSource.DataAnnotation));
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
            return relationshipBuilder.Metadata.DeclaringEntityType.HasDefiningNavigation()
                   || relationshipBuilder.Metadata.DeclaringEntityType.IsOwned()
                   || relationshipBuilder.Metadata.PrincipalEntityType.HasDefiningNavigation()
                   || relationshipBuilder.Metadata.PrincipalEntityType.IsOwned()
                ? relationshipBuilder
                : ConfigureInverseNavigation(
                    navigation.DeclaringEntityType.Builder, navigation.GetIdentifyingMemberInfo(), navigation.GetTargetType().Builder, attribute);
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
            var targetEntityType = RelationshipDiscoveryConvention.GetTargetEntityTypeBuilder(
                entityTypeBuilder, targetClrType, navigationPropertyInfo, null)?.Metadata;
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
                            _logger.MultipleInversePropertiesSameTargetWarning(
                                new[]
                                {
                                    Tuple.Create(referencingNavigationWithAttribute.Item1, referencingNavigationWithAttribute.Item2.ClrType),
                                    Tuple.Create(ambiguousInverse.Value.Item1, ambiguousInverse.Value.Item2.ClrType)
                                },
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
                if (inverseNavigation.Key.GetMemberType().IsAssignableFrom(entityType.ClrType)
                    && IsAmbiguousInverse(navigation, entityType, entityType.Model, inverseNavigation.Value))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsAmbiguousInverse(
            MemberInfo navigation,
            EntityType entityType,
            Model model,
            List<(MemberInfo, EntityType)> referencingNavigationsWithAttribute)
            => FindAmbiguousInverse(navigation, entityType, model, referencingNavigationsWithAttribute) != null;

        private static (MemberInfo, EntityType)? FindAmbiguousInverse(
            MemberInfo navigation,
            EntityType entityType,
            Model model,
            List<(MemberInfo, EntityType)> referencingNavigationsWithAttribute)
        {
            if (referencingNavigationsWithAttribute.Count == 1)
            {
                return null;
            }

            List<(MemberInfo, EntityType)> tuplesToRemove = null;
            (MemberInfo, EntityType)? ambiguousTuple = null;
            foreach (var referencingTuple in referencingNavigationsWithAttribute)
            {
                var inverseTargetEntityType = model.FindActualEntityType(referencingTuple.Item2);
                if ((inverseTargetEntityType?.Builder.IsIgnored(referencingTuple.Item1.GetSimpleMemberName(), ConfigurationSource.DataAnnotation) != false))
                {
                    if (tuplesToRemove == null)
                    {
                        tuplesToRemove = new List<(MemberInfo, EntityType)>();
                    }

                    tuplesToRemove.Add(referencingTuple);
                    continue;
                }

                if (!referencingTuple.Item1.IsSameAs(navigation)
                    || !entityType.IsSameHierarchy(inverseTargetEntityType))
                {
                    ambiguousTuple = referencingTuple;
                    break;
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

        private static List<(MemberInfo, EntityType)> AddInverseNavigation(
            EntityType entityType, MemberInfo navigation, EntityType targetEntityType, MemberInfo inverseNavigation)
        {
            var inverseNavigations = GetInverseNavigations(targetEntityType);
            if (inverseNavigations == null)
            {
                inverseNavigations = new Dictionary<MemberInfo, List<(MemberInfo, EntityType)>>();
                SetInverseNavigations(targetEntityType.Builder, inverseNavigations);
            }

            if (!inverseNavigations.TryGetValue(inverseNavigation, out var referencingNavigationsWithAttribute))
            {
                referencingNavigationsWithAttribute = new List<(MemberInfo, EntityType)>();
                inverseNavigations[inverseNavigation] = referencingNavigationsWithAttribute;
            }

            foreach (var referencingTuple in referencingNavigationsWithAttribute)
            {
                if (referencingTuple.Item1.IsSameAs(navigation)
                    && referencingTuple.Item2.ClrType == entityType.ClrType
                    && entityType.Model.FindActualEntityType(referencingTuple.Item2) == entityType)
                {
                    return referencingNavigationsWithAttribute;
                }
            }

            referencingNavigationsWithAttribute.Add((navigation, entityType));

            return referencingNavigationsWithAttribute;
        }

        private static void RemoveInverseNavigation(
            EntityType entityType,
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
                        && referencingTuple.Item2.ClrType == entityType.ClrType
                        && entityType.Model.FindActualEntityType(referencingTuple.Item2) == entityType)
                    {
                        referencingNavigationsWithAttribute.RemoveAt(index);
                        if (referencingNavigationsWithAttribute.Count == 0)
                        {
                            inverseNavigations.Remove(inverseNavigation);
                        }

                        if (referencingNavigationsWithAttribute.Count == 1)
                        {
                            var otherEntityType = entityType.Model.FindActualEntityType(referencingNavigationsWithAttribute[0].Item2);
                            if (otherEntityType != null)
                            {
                                targetEntityType.Builder.Relationship(
                                    otherEntityType.Builder,
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

        private static Dictionary<MemberInfo, List<(MemberInfo, EntityType)>> GetInverseNavigations(
            ConventionalAnnotatable entityType)
            => entityType.FindAnnotation(InverseNavigationsAnnotationName)?.Value
                as Dictionary<MemberInfo, List<(MemberInfo, EntityType)>>;

        private static void SetInverseNavigations(
            InternalMetadataBuilder entityTypeBuilder,
            Dictionary<MemberInfo, List<(MemberInfo, EntityType)>> inverseNavigations)
            => entityTypeBuilder.HasAnnotation(InverseNavigationsAnnotationName, inverseNavigations, ConfigurationSource.Convention);
    }
}
