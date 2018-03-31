// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class RelationshipDiscoveryConvention :
        IEntityTypeAddedConvention,
        IBaseTypeChangedConvention,
        INavigationRemovedConvention,
        IEntityTypeMemberIgnoredConvention,
        INavigationAddedConvention
    {
        private readonly ITypeMappingSource _typeMappingSource;
        private readonly IParameterBindingFactories _parameterBindingFactories;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Model> _logger;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationshipDiscoveryConvention(
            [NotNull] ITypeMappingSource typeMappingSource,
            [NotNull] IParameterBindingFactories parameterBindingFactories,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(parameterBindingFactories, nameof(parameterBindingFactories));

            _typeMappingSource = typeMappingSource;
            _parameterBindingFactories = parameterBindingFactories;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public const string NavigationCandidatesAnnotationName = "RelationshipDiscoveryConvention:NavigationCandidates";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public const string AmbiguousNavigationsAnnotationName = "RelationshipDiscoveryConvention:AmbiguousNavigations";

        private InternalEntityTypeBuilder DiscoverRelationships(InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (!entityTypeBuilder.Metadata.HasClrType()
                || entityTypeBuilder.ModelBuilder.IsIgnored(entityTypeBuilder.Metadata.ClrType, ConfigurationSource.Convention))
            {
                return entityTypeBuilder;
            }

            using (entityTypeBuilder.Metadata.Model.ConventionDispatcher.StartBatch())
            {
                var relationshipCandidates = FindRelationshipCandidates(entityTypeBuilder);
                relationshipCandidates = RemoveIncompatibleWithExistingRelationships(relationshipCandidates, entityTypeBuilder);
                relationshipCandidates = RemoveInheritedInverseNavigations(relationshipCandidates);
                relationshipCandidates = RemoveSingleSidedBaseNavigations(relationshipCandidates, entityTypeBuilder);
                CreateRelationships(relationshipCandidates, entityTypeBuilder);
            }

            return entityTypeBuilder;
        }

        private IReadOnlyList<RelationshipCandidate> FindRelationshipCandidates(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var relationshipCandidates = new Dictionary<EntityType, RelationshipCandidate>();
            var ownership = entityTypeBuilder.Metadata.FindOwnership();
            if (ownership == null
                && entityTypeBuilder.Metadata.Model.ShouldBeOwnedType(entityTypeBuilder.Metadata.ClrType))
            {
                return relationshipCandidates.Values.ToList();
            }

            var navigationCandidates = GetNavigationCandidates(entityTypeBuilder.Metadata);
            foreach (var candidateTuple in navigationCandidates)
            {
                var navigationPropertyInfo = candidateTuple.Key;
                var targetClrType = candidateTuple.Value;

                if (!IsCandidateNavigationProperty(entityTypeBuilder, navigationPropertyInfo.Name, navigationPropertyInfo))
                {
                    continue;
                }

                InternalEntityTypeBuilder candidateTargetEntityTypeBuilder = null;
                if (!entityTypeBuilder.ModelBuilder.Metadata.HasEntityTypeWithDefiningNavigation(targetClrType))
                {
                    candidateTargetEntityTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(targetClrType, ConfigurationSource.Convention);
                }
                else if (!targetClrType.GetTypeInfo().Equals(entityTypeBuilder.Metadata.ClrType.GetTypeInfo())
                         && !entityTypeBuilder.Metadata.IsInDefinitionPath(targetClrType))
                {
                    candidateTargetEntityTypeBuilder =
                        entityTypeBuilder.Metadata.FindNavigation(navigationPropertyInfo.Name)?.GetTargetType().Builder
                        ?? entityTypeBuilder.ModelBuilder.Metadata.FindEntityType(
                            targetClrType, navigationPropertyInfo.Name, entityTypeBuilder.Metadata)?.Builder
                        ?? entityTypeBuilder.ModelBuilder.Entity(
                            targetClrType, navigationPropertyInfo.Name, entityTypeBuilder.Metadata, ConfigurationSource.Convention);
                }

                if (candidateTargetEntityTypeBuilder == null
                    || (entityTypeBuilder.ModelBuilder.Metadata.ShouldBeOwnedType(entityTypeBuilder.Metadata.ClrType)
                        && candidateTargetEntityTypeBuilder.Metadata == entityTypeBuilder.Metadata))
                {
                    continue;
                }

                var candidateTargetEntityType = candidateTargetEntityTypeBuilder.Metadata;
                if (candidateTargetEntityType.IsQueryType)
                {
                    continue;
                }

                if (!entityTypeBuilder.Metadata.Model.ShouldBeOwnedType(candidateTargetEntityType.ClrType))
                {
                    var targetOwnership = candidateTargetEntityType.FindOwnership();
                    if (targetOwnership != null
                        && (targetOwnership.PrincipalEntityType != entityTypeBuilder.Metadata
                            || targetOwnership.PrincipalToDependent.Name != navigationPropertyInfo.Name))
                    {
                        if (ownership == null
                            || ownership.PrincipalEntityType != candidateTargetEntityType)
                        {
                            continue;
                        }
                    }
                } else if (// #8172
                    //ownership != null &&
                    navigationPropertyInfo.PropertyType.TryGetSequenceType() != null)
                {
                    continue;
                }

                var entityType = entityTypeBuilder.Metadata;

                if (relationshipCandidates.TryGetValue(candidateTargetEntityType, out var existingCandidate))
                {
                    if (candidateTargetEntityType != entityType
                        || !existingCandidate.InverseProperties.Contains(navigationPropertyInfo))
                    {
                        if (!existingCandidate.NavigationProperties.Contains(navigationPropertyInfo))
                        {
                            existingCandidate.NavigationProperties.Add(navigationPropertyInfo);
                        }
                    }

                    continue;
                }

                var navigations = new List<PropertyInfo>
                {
                    navigationPropertyInfo
                };
                var inverseCandidates = GetNavigationCandidates(candidateTargetEntityType);
                var inverseNavigationCandidates = new List<PropertyInfo>();

                foreach (var inverseCandidateTuple in inverseCandidates)
                {
                    var inversePropertyInfo = inverseCandidateTuple.Key;
                    var inverseTargetType = inverseCandidateTuple.Value;

                    if (inverseTargetType != entityType.ClrType
                        || navigationPropertyInfo.IsSameAs(inversePropertyInfo)
                        || entityType.IsQueryType
                        || (ownership != null
                            && (ownership.PrincipalEntityType != candidateTargetEntityType
                                || ownership.PrincipalToDependent.Name != inversePropertyInfo.Name))
                        || (entityType.HasDefiningNavigation()
                            && (entityType.DefiningEntityType != candidateTargetEntityType
                                || entityType.DefiningNavigationName != inversePropertyInfo.Name))
                        || !IsCandidateNavigationProperty(
                            candidateTargetEntityTypeBuilder, inversePropertyInfo.Name, inversePropertyInfo))
                    {
                        continue;
                    }

                    if (!inverseNavigationCandidates.Contains(inversePropertyInfo))
                    {
                        inverseNavigationCandidates.Add(inversePropertyInfo);
                    }
                }

                relationshipCandidates[candidateTargetEntityType] =
                    new RelationshipCandidate(candidateTargetEntityTypeBuilder, navigations, inverseNavigationCandidates);
            }

            return relationshipCandidates.Values.ToList();
        }

        private static IReadOnlyList<RelationshipCandidate> RemoveIncompatibleWithExistingRelationships(
            IReadOnlyList<RelationshipCandidate> relationshipCandidates,
            InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (relationshipCandidates.Count == 0)
            {
                return relationshipCandidates;
            }

            var filteredRelationshipCandidates = new List<RelationshipCandidate>();
            foreach (var relationshipCandidate in relationshipCandidates)
            {
                var targetEntityTypeBuilder = relationshipCandidate.TargetTypeBuilder;
                while (relationshipCandidate.NavigationProperties.Count > 0)
                {
                    var navigationProperty = relationshipCandidate.NavigationProperties[0];
                    var existingNavigation = entityTypeBuilder.Metadata.FindNavigation(navigationProperty.Name);
                    if (existingNavigation != null
                        && (existingNavigation.DeclaringEntityType != entityTypeBuilder.Metadata
                            || existingNavigation.GetTargetType() != targetEntityTypeBuilder.Metadata))
                    {
                        relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                        continue;
                    }

                    if (relationshipCandidate.NavigationProperties.Count == 1
                        && relationshipCandidate.InverseProperties.Count == 0)
                    {
                        break;
                    }

                    PropertyInfo compatibleInverse = null;
                    foreach (var inverseProperty in relationshipCandidate.InverseProperties)
                    {
                        if (IsCompatibleInverse(
                            navigationProperty, inverseProperty, entityTypeBuilder, targetEntityTypeBuilder))
                        {
                            if (compatibleInverse == null)
                            {
                                compatibleInverse = inverseProperty;
                            }
                            else
                            {
                                goto NextCandidate;
                            }
                        }
                    }

                    if (compatibleInverse == null)
                    {
                        relationshipCandidate.NavigationProperties.Remove(navigationProperty);

                        filteredRelationshipCandidates.Add(
                            new RelationshipCandidate(
                                targetEntityTypeBuilder,
                                new List<PropertyInfo>
                                {
                                    navigationProperty
                                },
                                new List<PropertyInfo>()));

                        if (relationshipCandidate.TargetTypeBuilder.Metadata == entityTypeBuilder.Metadata
                            && relationshipCandidate.InverseProperties.Count > 0)
                        {
                            var nextSelfRefCandidate = relationshipCandidate.InverseProperties.First();
                            if (!relationshipCandidate.NavigationProperties.Contains(nextSelfRefCandidate))
                            {
                                relationshipCandidate.NavigationProperties.Add(nextSelfRefCandidate);
                            }
                            relationshipCandidate.InverseProperties.Remove(nextSelfRefCandidate);
                        }
                        continue;
                    }

                    var noOtherCompatibleNavigation = true;
                    foreach (var n in relationshipCandidate.NavigationProperties)
                    {
                        if (n != navigationProperty
                            && IsCompatibleInverse(n, compatibleInverse, entityTypeBuilder, targetEntityTypeBuilder))
                        {
                            noOtherCompatibleNavigation = false;
                            break;
                        }
                    }

                    if (noOtherCompatibleNavigation)
                    {
                        relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                        relationshipCandidate.InverseProperties.Remove(compatibleInverse);

                        filteredRelationshipCandidates.Add(
                            new RelationshipCandidate(
                                targetEntityTypeBuilder,
                                new List<PropertyInfo>
                                {
                                    navigationProperty
                                },
                                new List<PropertyInfo>
                                {
                                    compatibleInverse
                                })
                            );

                        if (relationshipCandidate.TargetTypeBuilder.Metadata == entityTypeBuilder.Metadata
                            && relationshipCandidate.NavigationProperties.Count == 0
                            && relationshipCandidate.InverseProperties.Count > 0)
                        {
                            var nextSelfRefCandidate = relationshipCandidate.InverseProperties.First();
                            if (!relationshipCandidate.NavigationProperties.Contains(nextSelfRefCandidate))
                            {
                                relationshipCandidate.NavigationProperties.Add(nextSelfRefCandidate);
                            }
                            relationshipCandidate.InverseProperties.Remove(nextSelfRefCandidate);
                        }
                        continue;
                    }

                    NextCandidate:
                    break;
                }

                if (relationshipCandidate.NavigationProperties.Count > 0
                    || relationshipCandidate.InverseProperties.Count > 0)
                {
                    filteredRelationshipCandidates.Add(relationshipCandidate);
                }
                else if (relationshipCandidate.TargetTypeBuilder.Metadata.HasDefiningNavigation()
                         && filteredRelationshipCandidates.All(
                             c => c.TargetTypeBuilder.Metadata != relationshipCandidate.TargetTypeBuilder.Metadata))
                {
                    entityTypeBuilder.ModelBuilder
                        .RemoveEntityType(relationshipCandidate.TargetTypeBuilder.Metadata, ConfigurationSource.Convention);
                }
            }

            return filteredRelationshipCandidates;
        }

        private static bool IsCompatibleInverse(
            PropertyInfo navigationProperty,
            PropertyInfo inversePropertyInfo,
            InternalEntityTypeBuilder entityTypeBuilder,
            InternalEntityTypeBuilder targetEntityTypeBuilder)
        {
            var entityType = entityTypeBuilder.Metadata;
            var existingNavigation = entityType.FindNavigation(navigationProperty.Name);
            if (existingNavigation != null
                && !CanMergeWith(existingNavigation, inversePropertyInfo, targetEntityTypeBuilder))
            {
                return false;
            }

            var existingInverse = targetEntityTypeBuilder.Metadata.FindNavigation(inversePropertyInfo.Name);
            if (existingInverse != null)
            {
                if (existingInverse.DeclaringEntityType != targetEntityTypeBuilder.Metadata
                    || !CanMergeWith(existingInverse, navigationProperty, entityTypeBuilder))
                {
                    return false;
                }

                var otherEntityType = existingInverse.GetTargetType();
                if (!entityType.ClrType.GetTypeInfo()
                    .IsAssignableFrom(otherEntityType.ClrType.GetTypeInfo()))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CanMergeWith(
            Navigation existingNavigation, PropertyInfo inverse, InternalEntityTypeBuilder inverseEntityTypeBuilder)
        {
            var fk = existingNavigation.ForeignKey;
            return (fk.IsSelfReferencing()
                    || fk.ResolveOtherEntityType(existingNavigation.DeclaringEntityType) == inverseEntityTypeBuilder.Metadata)
                   && fk.Builder.CanSetNavigation(inverse, !existingNavigation.IsDependentToPrincipal(), ConfigurationSource.Convention);
        }

        private static IReadOnlyList<RelationshipCandidate> RemoveInheritedInverseNavigations(
            IReadOnlyList<RelationshipCandidate> relationshipCandidates)
        {
            if (relationshipCandidates.Count == 0)
            {
                return relationshipCandidates;
            }

            var relationshipCandidatesByRoot = relationshipCandidates.GroupBy(r => r.TargetTypeBuilder.Metadata.RootType())
                .ToDictionary(g => g.Key, g => g.ToList());
            foreach (var relationshipCandidatesHierarchy in relationshipCandidatesByRoot.Values)
            {
                var filteredRelationshipCandidates = new HashSet<RelationshipCandidate>();
                foreach (var relationshipCandidate in relationshipCandidatesHierarchy)
                {
                    RemoveInheritedInverseNavigations(
                        relationshipCandidate, relationshipCandidatesHierarchy, filteredRelationshipCandidates);
                }
            }

            return relationshipCandidates;
        }

        private static void RemoveInheritedInverseNavigations(
            RelationshipCandidate relationshipCandidate,
            List<RelationshipCandidate> relationshipCandidatesHierarchy,
            HashSet<RelationshipCandidate> filteredRelationshipCandidates)
        {
            if (filteredRelationshipCandidates.Contains(relationshipCandidate)
                || relationshipCandidate.NavigationProperties.Count > 1
                && relationshipCandidate.InverseProperties.Count > 0
                || relationshipCandidate.InverseProperties.Count > 1)
            {
                return;
            }

            filteredRelationshipCandidates.Add(relationshipCandidate);
            var inverseCandidate = relationshipCandidate.InverseProperties.FirstOrDefault();
            if (inverseCandidate != null)
            {
                var relationshipsToDerivedTypes = relationshipCandidatesHierarchy
                    .Where(
                        r => r.TargetTypeBuilder != relationshipCandidate.TargetTypeBuilder
                             && relationshipCandidate.TargetTypeBuilder.Metadata.IsAssignableFrom(r.TargetTypeBuilder.Metadata));
                foreach (var relationshipToDerivedType in relationshipsToDerivedTypes)
                {
                    relationshipToDerivedType.InverseProperties.RemoveAll(i => i.Name == inverseCandidate.Name);

                    if (!filteredRelationshipCandidates.Contains(relationshipToDerivedType))
                    {
                        // An ambiguity might have been resolved
                        RemoveInheritedInverseNavigations(relationshipToDerivedType, relationshipCandidatesHierarchy, filteredRelationshipCandidates);
                    }
                }
            }
        }

        private static IReadOnlyList<RelationshipCandidate> RemoveSingleSidedBaseNavigations(
            IReadOnlyList<RelationshipCandidate> relationshipCandidates,
            InternalEntityTypeBuilder entityTypeBuilder)
        {
            if (relationshipCandidates.Count == 0)
            {
                return relationshipCandidates;
            }

            var filteredRelationshipCandidates = new List<RelationshipCandidate>();
            foreach (var relationshipCandidate in relationshipCandidates)
            {
                if (relationshipCandidate.InverseProperties.Count > 0)
                {
                    filteredRelationshipCandidates.Add(relationshipCandidate);
                    continue;
                }

                foreach (var navigation in relationshipCandidate.NavigationProperties.ToList())
                {
                    if (entityTypeBuilder.Metadata.FindDerivedNavigations(navigation.Name).Any(n => n.FindInverse() != null))
                    {
                        relationshipCandidate.NavigationProperties.Remove(navigation);
                    }
                }

                if (relationshipCandidate.NavigationProperties.Count > 0)
                {
                    filteredRelationshipCandidates.Add(relationshipCandidate);
                }
                else if (relationshipCandidate.TargetTypeBuilder.Metadata.HasDefiningNavigation()
                         && filteredRelationshipCandidates.All(
                             c => c.TargetTypeBuilder.Metadata != relationshipCandidate.TargetTypeBuilder.Metadata))
                {
                    entityTypeBuilder.ModelBuilder
                        .RemoveEntityType(relationshipCandidate.TargetTypeBuilder.Metadata, ConfigurationSource.Convention);
                }
            }

            return filteredRelationshipCandidates;
        }

        private void CreateRelationships(
            IEnumerable<RelationshipCandidate> relationshipCandidates, InternalEntityTypeBuilder entityTypeBuilder)
        {
            var unusedEntityTypes = new List<EntityType>();
            foreach (var relationshipCandidate in relationshipCandidates)
            {
                var entityType = entityTypeBuilder.Metadata;
                var targetEntityType = relationshipCandidate.TargetTypeBuilder.Metadata;
                var isAmbiguousOnBase = entityType.BaseType != null
                                        && HasAmbiguousNavigationsTo(
                                            entityType.BaseType, targetEntityType.ClrType)
                                        || targetEntityType.BaseType != null
                                        && HasAmbiguousNavigationsTo(
                                            targetEntityType.BaseType, entityType.ClrType);

                if ((relationshipCandidate.NavigationProperties.Count > 1
                     && relationshipCandidate.InverseProperties.Count > 0
                     && (!targetEntityType.Model.ShouldBeOwnedType(targetEntityType.ClrType)
                         || entityType.IsInOwnershipPath(targetEntityType)))
                    || relationshipCandidate.InverseProperties.Count > 1
                    || isAmbiguousOnBase
                    || HasDeclaredAmbiguousNavigationsTo(entityType, targetEntityType.ClrType)
                    || HasDeclaredAmbiguousNavigationsTo(targetEntityType, entityType.ClrType))
                {
                    if (!isAmbiguousOnBase)
                    {
                        AddAmbiguous(entityTypeBuilder, relationshipCandidate.NavigationProperties, targetEntityType.ClrType);

                        AddAmbiguous(targetEntityType.Builder, relationshipCandidate.InverseProperties, entityType.ClrType);

                        _logger.MultipleNavigationProperties(
                            relationshipCandidate.NavigationProperties.Count == 0
                                ? new[] { new Tuple<MemberInfo, Type>(null, targetEntityType.ClrType) }
                                : relationshipCandidate.NavigationProperties.Select(n => new Tuple<MemberInfo, Type>(n, entityType.ClrType)),
                            relationshipCandidate.InverseProperties.Count == 0
                                ? new[] { new Tuple<MemberInfo, Type>(null, targetEntityType.ClrType) }
                                : relationshipCandidate.InverseProperties.Select(n => new Tuple<MemberInfo, Type>(n, targetEntityType.ClrType)));
                    }

                    foreach (var navigationProperty in relationshipCandidate.NavigationProperties)
                    {
                        var existingForeignKey = entityType.FindDeclaredNavigation(navigationProperty.Name)?.ForeignKey;
                        existingForeignKey?.DeclaringEntityType.Builder
                            .RemoveForeignKey(existingForeignKey, ConfigurationSource.Convention);
                    }

                    foreach (var inverseProperty in relationshipCandidate.InverseProperties)
                    {
                        var existingForeignKey = targetEntityType.FindDeclaredNavigation(inverseProperty.Name)?.ForeignKey;
                        existingForeignKey?.DeclaringEntityType.Builder
                            .RemoveForeignKey(existingForeignKey, ConfigurationSource.Convention);
                    }

                    unusedEntityTypes.Add(targetEntityType);

                    continue;
                }

                foreach (var navigation in relationshipCandidate.NavigationProperties)
                {
                    if (targetEntityType.Builder == null
                        && !targetEntityType.Model.ShouldBeOwnedType(targetEntityType.ClrType))
                    {
                        continue;
                    }

                    if (InversePropertyAttributeConvention.IsAmbiguous(entityType, navigation, targetEntityType))
                    {
                        unusedEntityTypes.Add(targetEntityType);
                        continue;
                    }

                    var inverse = relationshipCandidate.InverseProperties.SingleOrDefault();
                    if (inverse == null)
                    {
                        if (targetEntityType.Model.ShouldBeOwnedType(targetEntityType.ClrType)
                            && !entityType.IsInOwnershipPath(targetEntityType))
                        {
                            entityTypeBuilder.Owns(
                                targetEntityType.ClrType,
                                navigation,
                                ConfigurationSource.Convention);
                        }
                        else
                        {
                            entityTypeBuilder.Navigation(
                                targetEntityType.Builder,
                                navigation,
                                ConfigurationSource.Convention);
                        }
                    }
                    else
                    {
                        if (InversePropertyAttributeConvention.IsAmbiguous(targetEntityType, inverse, entityType))
                        {
                            unusedEntityTypes.Add(targetEntityType);
                            continue;
                        }

                        if (targetEntityType.Model.ShouldBeOwnedType(targetEntityType.ClrType)
                            && !entityType.IsInOwnershipPath(targetEntityType))
                        {
                            entityTypeBuilder.Owns(
                                targetEntityType.ClrType,
                                navigation,
                                inverse,
                                ConfigurationSource.Convention);
                        }
                        else
                        {
                            entityTypeBuilder.Relationship(
                                targetEntityType.Builder,
                                navigation,
                                inverse,
                                ConfigurationSource.Convention);
                        }
                    }
                }

                if (relationshipCandidate.NavigationProperties.Count == 0)
                {
                    foreach (var inverse in relationshipCandidate.InverseProperties)
                    {
                        if (targetEntityType.Builder == null)
                        {
                            continue;
                        }

                        if (InversePropertyAttributeConvention.IsAmbiguous(targetEntityType, inverse, entityType))
                        {
                            unusedEntityTypes.Add(targetEntityType);
                            continue;
                        }

                        targetEntityType.Builder.Navigation(
                            entityTypeBuilder,
                            inverse,
                            ConfigurationSource.Convention);
                    }
                }
            }

            foreach (var unusedEntityType in unusedEntityTypes)
            {
                if (unusedEntityType.HasDefiningNavigation()
                    && unusedEntityType.DefiningEntityType.FindNavigation(unusedEntityType.DefiningNavigationName) == null)
                {
                    entityTypeBuilder.ModelBuilder.RemoveEntityType(unusedEntityType, ConfigurationSource.Convention);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
            => !entityTypeBuilder.Metadata.HasClrType() ? entityTypeBuilder : DiscoverRelationships(entityTypeBuilder);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, EntityType oldBaseType)
        {
            var oldBaseTypeBuilder = oldBaseType?.Builder;
            if (oldBaseTypeBuilder != null)
            {
                DiscoverRelationships(oldBaseTypeBuilder);
            }

            ApplyOnRelatedEntityTypes(entityTypeBuilder.Metadata);
            foreach (var entityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
            {
                DiscoverRelationships(entityType.Builder);
            }

            return true;
        }

        private void ApplyOnRelatedEntityTypes(EntityType entityType)
        {
            var relatedEntityTypes = entityType.GetReferencingForeignKeys().Select(fk => fk.DeclaringEntityType)
                .Concat(entityType.GetForeignKeys().Select(fk => fk.PrincipalEntityType))
                .Distinct()
                .ToList();

            foreach (var relatedEntityType in relatedEntityTypes)
            {
                var relatedEntityTypeBuilder = relatedEntityType.Builder;
                DiscoverRelationships(relatedEntityTypeBuilder);
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(
            InternalEntityTypeBuilder sourceEntityTypeBuilder,
            InternalEntityTypeBuilder targetEntityTypeBuilder,
            string navigationName,
            MemberInfo propertyInfo)
        {
            if ((targetEntityTypeBuilder.Metadata.Builder == null
                 && sourceEntityTypeBuilder.ModelBuilder.IsIgnored(
                     targetEntityTypeBuilder.Metadata.Name, ConfigurationSource.Convention))
                || !IsCandidateNavigationProperty(sourceEntityTypeBuilder, navigationName, propertyInfo))
            {
                return true;
            }

            return Apply(sourceEntityTypeBuilder.Metadata, propertyInfo);
        }

        private bool Apply(EntityType entityType, MemberInfo navigationProperty)
        {
            DiscoverRelationships(entityType.Builder);
            if (entityType.FindNavigation(navigationProperty.Name) != null)
            {
                return false;
            }

            if (IsAmbiguous(entityType, navigationProperty))
            {
                return true;
            }

            foreach (var derivedEntityType in entityType.GetDirectlyDerivedTypes())
            {
                Apply(derivedEntityType, navigationProperty);
            }

            return true;
        }

        [ContractAnnotation("propertyInfo:null => false")]
        private static bool IsCandidateNavigationProperty(
            InternalEntityTypeBuilder sourceEntityTypeBuilder, string navigationName, MemberInfo propertyInfo)
            => propertyInfo != null
               && sourceEntityTypeBuilder != null
               && !sourceEntityTypeBuilder.IsIgnored(navigationName, ConfigurationSource.Convention)
               && sourceEntityTypeBuilder.Metadata.FindProperty(navigationName) == null
               && sourceEntityTypeBuilder.Metadata.FindServiceProperty(navigationName) == null
               && (!sourceEntityTypeBuilder.Metadata.IsQueryType
                   || (propertyInfo as PropertyInfo)?.PropertyType.TryGetSequenceType() == null);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, string ignoredMemberName)
        {
            var anyAmbiguityRemoved = false;
            foreach (var derivedEntityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
            {
                var ambigousNavigations = GetAmbigousNavigations(derivedEntityType);
                if (ambigousNavigations == null)
                {
                    continue;
                }

                KeyValuePair<MemberInfo, Type>? ambigousNavigation = null;
                foreach (var navigation in ambigousNavigations)
                {
                    if (navigation.Key.Name == ignoredMemberName)
                    {
                        ambigousNavigation = navigation;
                    }
                }

                if (ambigousNavigation == null)
                {
                    continue;
                }

                anyAmbiguityRemoved = true;

                var targetClrType = ambigousNavigation.Value.Value;
                RemoveAmbiguous(derivedEntityType, targetClrType);

                var targetType = entityTypeBuilder.Metadata.Model.FindEntityType(targetClrType);
                if (targetType != null)
                {
                    RemoveAmbiguous(targetType, derivedEntityType.ClrType);
                }
            }

            if (anyAmbiguityRemoved)
            {
                DiscoverRelationships(entityTypeBuilder);
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder, Navigation navigation)
        {
            foreach (var entityType in navigation.DeclaringEntityType.GetDerivedTypesInclusive())
            {
                // Only run the convention if an ambiguity might have been removed
                if (RemoveAmbiguous(entityType, navigation.GetTargetType().ClrType)
                    | RemoveAmbiguous(navigation.GetTargetType(), entityType.ClrType))
                {
                    DiscoverRelationships(entityType.Builder);
                }
            }

            if (relationshipBuilder.Metadata.Builder == null)
            {
                relationshipBuilder = navigation.DeclaringEntityType.FindNavigation(navigation.Name)?.ForeignKey?.Builder;
            }

            return relationshipBuilder;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Type FindCandidateNavigationPropertyType([NotNull] PropertyInfo propertyInfo)
            => Check.NotNull(propertyInfo, nameof(propertyInfo)).FindCandidateNavigationPropertyType(_typeMappingSource, _parameterBindingFactories);

        private ImmutableSortedDictionary<PropertyInfo, Type> GetNavigationCandidates(EntityType entityType)
        {
            if (entityType.FindAnnotation(NavigationCandidatesAnnotationName)?.Value
                is ImmutableSortedDictionary<PropertyInfo, Type> navigationCandidates)
            {
                return navigationCandidates;
            }

            var dictionaryBuilder = ImmutableSortedDictionary.CreateBuilder<PropertyInfo, Type>(MemberInfoNameComparer.Instance);
            if (entityType.HasClrType())
            {
                foreach (var propertyInfo in entityType.GetRuntimeProperties().Values.OrderBy(p => p.Name))
                {
                    var targetType = FindCandidateNavigationPropertyType(propertyInfo);
                    if (targetType != null)
                    {
                        dictionaryBuilder[propertyInfo] = targetType;
                    }
                }
            }
            navigationCandidates = dictionaryBuilder.ToImmutable();
            SetNavigationCandidates(entityType.Builder, navigationCandidates);
            return navigationCandidates;
        }

        private static void SetNavigationCandidates(
            InternalEntityTypeBuilder entityTypeBuilder,
            ImmutableSortedDictionary<PropertyInfo, Type> navigationCandidates)
            => entityTypeBuilder.HasAnnotation(NavigationCandidatesAnnotationName, navigationCandidates, ConfigurationSource.Convention);

        private static bool IsAmbiguous(EntityType entityType, MemberInfo navigationProperty)
        {
            while (entityType != null)
            {
                var ambigousNavigations = GetAmbigousNavigations(entityType);
                if (ambigousNavigations != null
                    && ambigousNavigations.ContainsKey(navigationProperty))
                {
                    return true;
                }

                entityType = entityType.BaseType;
            }

            return false;
        }

        private static bool HasAmbiguousNavigationsTo(EntityType sourceEntityType, Type targetClrType)
        {
            while (sourceEntityType != null)
            {
                if (HasDeclaredAmbiguousNavigationsTo(sourceEntityType, targetClrType))
                {
                    return true;
                }

                sourceEntityType = sourceEntityType.BaseType;
            }

            return false;
        }

        private static bool HasDeclaredAmbiguousNavigationsTo(EntityType sourceEntityType, Type targetClrType)
        {
            var ambigousNavigations = GetAmbigousNavigations(sourceEntityType);
            if (ambigousNavigations != null
                && ambigousNavigations.ContainsValue(targetClrType))
            {
                return true;
            }
            return false;
        }

        private static ImmutableSortedDictionary<MemberInfo, Type> GetAmbigousNavigations(EntityType entityType)
            => entityType.FindAnnotation(AmbiguousNavigationsAnnotationName)?.Value
                as ImmutableSortedDictionary<MemberInfo, Type>;

        private static void AddAmbiguous(
            InternalEntityTypeBuilder entityTypeBuilder, IEnumerable<PropertyInfo> navigationProperties, Type targetType)
        {
            var newAmbiguousNavigations = ImmutableSortedDictionary.CreateRange(
                MemberInfoNameComparer.Instance,
                navigationProperties.Select(n => new KeyValuePair<MemberInfo, Type>(n, targetType)));

            var currentAmbiguousNavigations = GetAmbigousNavigations(entityTypeBuilder.Metadata);
            if (currentAmbiguousNavigations != null)
            {
                newAmbiguousNavigations = currentAmbiguousNavigations.AddRange(newAmbiguousNavigations);
            }

            SetAmbigousNavigations(entityTypeBuilder, newAmbiguousNavigations);
        }

        private static bool RemoveAmbiguous(EntityType entityType, Type targetType)
        {
            var ambigousNavigations = GetAmbigousNavigations(entityType);
            if (ambigousNavigations != null)
            {
                var newAmbigousNavigations = ambigousNavigations;
                foreach (var ambigousNavigation in ambigousNavigations)
                {
                    if (ambigousNavigation.Value == targetType)
                    {
                        newAmbigousNavigations = newAmbigousNavigations.Remove(ambigousNavigation.Key);
                    }
                }

                if (ambigousNavigations.Count != newAmbigousNavigations.Count)
                {
                    SetAmbigousNavigations(entityType.Builder, newAmbigousNavigations);
                    return true;
                }
            }

            return false;
        }

        private static void SetAmbigousNavigations(
            InternalEntityTypeBuilder entityTypeBuilder,
            ImmutableSortedDictionary<MemberInfo, Type> ambiguousNavigations)
            => entityTypeBuilder.HasAnnotation(AmbiguousNavigationsAnnotationName, ambiguousNavigations, ConfigurationSource.Convention);

        private class MemberInfoNameComparer : IComparer<MemberInfo>
        {
            public static readonly MemberInfoNameComparer Instance = new MemberInfoNameComparer();

            private MemberInfoNameComparer()
            {
            }

            public int Compare(MemberInfo x, MemberInfo y) => StringComparer.Ordinal.Compare(x.Name, y.Name);
        }

        private class RelationshipCandidate
        {
            public RelationshipCandidate(
                InternalEntityTypeBuilder targetTypeBuilder,
                List<PropertyInfo> navigations,
                List<PropertyInfo> inverseNavigations)
            {
                TargetTypeBuilder = targetTypeBuilder;
                NavigationProperties = navigations;
                InverseProperties = inverseNavigations;
            }

            public InternalEntityTypeBuilder TargetTypeBuilder { [DebuggerStepThrough] get; }
            public List<PropertyInfo> NavigationProperties { [DebuggerStepThrough] get; }
            public List<PropertyInfo> InverseProperties { [DebuggerStepThrough] get; }
        }
    }
}
