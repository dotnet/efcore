// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        private readonly ICoreTypeMapper _typeMapper;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Model> _logger;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public RelationshipDiscoveryConvention(
            [NotNull] ICoreTypeMapper typeMapper,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
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
            var navigationCandidates = GetNavigationCandidates(entityTypeBuilder.Metadata);
            foreach (var candidateTuple in navigationCandidates)
            {
                var navigationPropertyInfo = candidateTuple.Key;
                var targetClrType = candidateTuple.Value;
                
                if (entityTypeBuilder.IsIgnored(navigationPropertyInfo.Name, ConfigurationSource.Convention)
                    || (entityTypeBuilder.Metadata.IsQueryType
                        && navigationPropertyInfo.PropertyType.TryGetSequenceType() != null))
                {
                    continue;
                }

                InternalEntityTypeBuilder candidateTargetEntityTypeBuilder = null;
                if (!entityTypeBuilder.ModelBuilder.Metadata.HasEntityTypeWithDefiningNavigation(targetClrType)
                    && !entityTypeBuilder.ModelBuilder.Metadata.ShouldBeOwnedType(targetClrType))
                {
                    candidateTargetEntityTypeBuilder = entityTypeBuilder.ModelBuilder.Entity(targetClrType, ConfigurationSource.Convention);
                } else if (!targetClrType.GetTypeInfo().Equals(entityTypeBuilder.Metadata.ClrType.GetTypeInfo())
                           && !entityTypeBuilder.Metadata.IsInDefinitionPath(targetClrType))
                {
                    candidateTargetEntityTypeBuilder =
                        entityTypeBuilder.Metadata.FindNavigation(navigationPropertyInfo.Name)?.GetTargetType().Builder
                        ?? entityTypeBuilder.ModelBuilder.Metadata.FindEntityType(
                            targetClrType, navigationPropertyInfo.Name, entityTypeBuilder.Metadata)?.Builder
                        ?? entityTypeBuilder.ModelBuilder.Entity(
                            targetClrType, navigationPropertyInfo.Name, entityTypeBuilder.Metadata, ConfigurationSource.Convention);
                }

                if (candidateTargetEntityTypeBuilder == null)
                {
                    continue;
                }

                var candidateTargetEntityType = candidateTargetEntityTypeBuilder.Metadata;

                if (candidateTargetEntityType.IsQueryType)
                {
                    continue;
                }
                
                var entityType = entityTypeBuilder.Metadata;

                if (relationshipCandidates.TryGetValue(candidateTargetEntityType, out var existingCandidate))
                {
                    if (candidateTargetEntityType != entityType
                        || !existingCandidate.InverseProperties.Contains(navigationPropertyInfo))
                    {
                        existingCandidate.NavigationProperties.Add(navigationPropertyInfo);
                    }

                    continue;
                }

                var navigations = new HashSet<PropertyInfo> { navigationPropertyInfo };
                var inverseCandidates = GetNavigationCandidates(candidateTargetEntityType);
                var inverseNavigationCandidates = new HashSet<PropertyInfo>();

                foreach (var inverseCandidateTuple in inverseCandidates)
                {
                    var inversePropertyInfo = inverseCandidateTuple.Key;
                    var inverseTargetType = inverseCandidateTuple.Value;

                    if (inverseTargetType != entityType.ClrType
                        || navigationPropertyInfo.IsSameAs(inversePropertyInfo)
                        || candidateTargetEntityTypeBuilder.IsIgnored(inversePropertyInfo.Name, ConfigurationSource.Convention)
                        || entityType.IsQueryType
                        || (entityType.HasDefiningNavigation()
                            && entityType.DefiningEntityType == candidateTargetEntityType
                            && entityType.DefiningNavigationName != inversePropertyInfo.Name))
                    {
                        continue;
                    }

                    inverseNavigationCandidates.Add(inversePropertyInfo);
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
                var revisitNavigations = true;
                while (revisitNavigations)
                {
                    revisitNavigations = false;
                    foreach (var navigationProperty in relationshipCandidate.NavigationProperties)
                    {
                        var existingNavigation = entityTypeBuilder.Metadata.FindNavigation(navigationProperty.Name);
                        if (existingNavigation != null
                            && (existingNavigation.DeclaringEntityType != entityTypeBuilder.Metadata
                                || existingNavigation.GetTargetType() != targetEntityTypeBuilder.Metadata))
                        {
                            relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                            revisitNavigations = true;
                            break;
                        }

                        if (relationshipCandidate.NavigationProperties.Count == 1
                            && relationshipCandidate.InverseProperties.Count == 0)
                        {
                            break;
                        }

                        var compatibleInverseProperties = relationshipCandidate.InverseProperties
                            .Where(i => IsCompatibleInverse(navigationProperty, i, entityTypeBuilder, targetEntityTypeBuilder))
                            .ToList();

                        if (compatibleInverseProperties.Count == 0)
                        {
                            relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                            revisitNavigations = true;

                            filteredRelationshipCandidates.Add(
                                new RelationshipCandidate(
                                    targetEntityTypeBuilder,
                                    new HashSet<PropertyInfo> { navigationProperty },
                                    new HashSet<PropertyInfo>()));

                            if (relationshipCandidate.TargetTypeBuilder.Metadata == entityTypeBuilder.Metadata
                                && relationshipCandidate.InverseProperties.Count > 0)
                            {
                                var nextSelfRefCandidate = relationshipCandidate.InverseProperties.First();
                                relationshipCandidate.NavigationProperties.Add(nextSelfRefCandidate);
                                relationshipCandidate.InverseProperties.Remove(nextSelfRefCandidate);
                            }
                        }
                        else if (compatibleInverseProperties.Count == 1)
                        {
                            var inverseProperty = compatibleInverseProperties[0];
                            if (!relationshipCandidate.NavigationProperties.Any(
                                n =>
                                    n != navigationProperty
                                    && IsCompatibleInverse(n, inverseProperty, entityTypeBuilder, targetEntityTypeBuilder)))
                            {
                                relationshipCandidate.NavigationProperties.Remove(navigationProperty);
                                relationshipCandidate.InverseProperties.Remove(inverseProperty);
                                revisitNavigations = true;

                                filteredRelationshipCandidates.Add(
                                    new RelationshipCandidate(
                                        targetEntityTypeBuilder,
                                        new HashSet<PropertyInfo> { navigationProperty },
                                        new HashSet<PropertyInfo> { inverseProperty }));

                                if (relationshipCandidate.TargetTypeBuilder.Metadata == entityTypeBuilder.Metadata
                                    && relationshipCandidate.NavigationProperties.Count == 0
                                    && relationshipCandidate.InverseProperties.Count > 0)
                                {
                                    var nextSelfRefCandidate = relationshipCandidate.InverseProperties.First();
                                    relationshipCandidate.NavigationProperties.Add(nextSelfRefCandidate);
                                    relationshipCandidate.InverseProperties.Remove(nextSelfRefCandidate);
                                }
                            }
                        }

                        break;
                    }
                }

                if (relationshipCandidate.NavigationProperties.Count > 0
                    || relationshipCandidate.InverseProperties.Count > 0)
                {
                    filteredRelationshipCandidates.Add(relationshipCandidate);
                }
                else if (relationshipCandidate.TargetTypeBuilder.Metadata.HasDefiningNavigation()
                         && filteredRelationshipCandidates.All(
                             c =>
                                 c.TargetTypeBuilder.Metadata != relationshipCandidate.TargetTypeBuilder.Metadata))
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
                && !CanMergeWith(existingNavigation, inversePropertyInfo.Name, targetEntityTypeBuilder))
            {
                return false;
            }

            var existingInverse = targetEntityTypeBuilder.Metadata.FindNavigation(inversePropertyInfo.Name);
            if (existingInverse != null)
            {
                if (existingInverse.DeclaringEntityType != targetEntityTypeBuilder.Metadata
                    || !CanMergeWith(existingInverse, navigationProperty.Name, entityTypeBuilder))
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
            Navigation existingNavigation, string inverseName, InternalEntityTypeBuilder inverseEntityTypeBuilder)
        {
            var fk = existingNavigation.ForeignKey;
            return (fk.IsSelfReferencing()
                    || fk.ResolveOtherEntityType(existingNavigation.DeclaringEntityType) == inverseEntityTypeBuilder.Metadata)
                   && fk.Builder.CanSetNavigation(inverseName, !existingNavigation.IsDependentToPrincipal(), ConfigurationSource.Convention);
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
                    relationshipToDerivedType.InverseProperties.RemoveWhere(i => i.Name == inverseCandidate.Name);

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
                             c =>
                                 c.TargetTypeBuilder.Metadata != relationshipCandidate.TargetTypeBuilder.Metadata))
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

                if (relationshipCandidate.NavigationProperties.Count > 1
                    && relationshipCandidate.InverseProperties.Count > 0
                    || relationshipCandidate.InverseProperties.Count > 1
                    || isAmbiguousOnBase)
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
                    if (targetEntityType.Builder == null)
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
                        if (targetEntityType.DefiningNavigationName == navigation.Name
                            && targetEntityType.DefiningEntityType == entityType
                            && targetEntityType.Model.ShouldBeOwnedType(targetEntityType.ClrType))
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

                        if (targetEntityType.DefiningNavigationName == navigation.Name
                            && targetEntityType.DefiningEntityType == entityType
                            && targetEntityType.Model.ShouldBeOwnedType(targetEntityType.ClrType))
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
            PropertyInfo propertyInfo)
        {
            sourceEntityTypeBuilder = sourceEntityTypeBuilder.Metadata.Builder;
            if (!IsCandidateNavigationProperty(sourceEntityTypeBuilder, navigationName, propertyInfo))
            {
                return true;
            }

            return Apply(sourceEntityTypeBuilder.Metadata, propertyInfo);
        }

        private bool Apply(EntityType entityType, PropertyInfo navigationProperty)
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
            InternalEntityTypeBuilder sourceEntityTypeBuilder, string navigationName, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                return false;
            }

            if (sourceEntityTypeBuilder == null
                || sourceEntityTypeBuilder.ModelBuilder.IsIgnored(sourceEntityTypeBuilder.Metadata.Name, ConfigurationSource.Convention))
            {
                return false;
            }

            if (sourceEntityTypeBuilder.IsIgnored(navigationName, ConfigurationSource.Convention))
            {
                return false;
            }

            if (sourceEntityTypeBuilder.Metadata.FindProperty(navigationName) != null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool Apply(InternalEntityTypeBuilder entityTypeBuilder, string ignoredMemberName)
        {
            foreach (var derivedEntityType in entityTypeBuilder.Metadata.GetDerivedTypesInclusive())
            {
                var ambigousNavigation = GetAmbigousNavigations(derivedEntityType)?.Keys.FirstOrDefault(p => p.Name == ignoredMemberName);
                if (ambigousNavigation == null)
                {
                    continue;
                }

                RemoveAmbiguous(derivedEntityType, ambigousNavigation);

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
                if (RemoveAmbiguous(entityType, navigation.PropertyInfo))
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
            => Check.NotNull(propertyInfo, nameof(propertyInfo)).FindCandidateNavigationPropertyType(
                t => _typeMapper.FindMapping(t) != null);

        private ImmutableSortedDictionary<PropertyInfo, Type> GetNavigationCandidates(EntityType entityType)
        {
            if (entityType.FindAnnotation(NavigationCandidatesAnnotationName)?.Value
                is ImmutableSortedDictionary<PropertyInfo, Type> navigationCandidates)
            {
                return navigationCandidates;
            }

            var dictionaryBuilder = ImmutableSortedDictionary.CreateBuilder<PropertyInfo, Type>(PropertyInfoNameComparer.Instance);
            if (entityType.HasClrType())
            {
                foreach (var propertyInfo in entityType.ClrType.GetRuntimeProperties().OrderBy(p => p.Name))
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

        private static bool IsAmbiguous(EntityType entityType, PropertyInfo navigationProperty)
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
                var ambigousNavigations = GetAmbigousNavigations(sourceEntityType);
                if (ambigousNavigations != null
                    && ambigousNavigations.ContainsValue(targetClrType))
                {
                    return true;
                }

                sourceEntityType = sourceEntityType.BaseType;
            }

            return false;
        }

        private static ImmutableSortedDictionary<PropertyInfo, Type> GetAmbigousNavigations(EntityType entityType)
            => entityType.FindAnnotation(AmbiguousNavigationsAnnotationName)?.Value
                as ImmutableSortedDictionary<PropertyInfo, Type>;

        private static void AddAmbiguous(InternalEntityTypeBuilder entityTypeBuilder, IEnumerable<PropertyInfo> navigationProperties, Type targetType)
        {
            var newAmbiguousNavigations = ImmutableSortedDictionary.CreateRange(
                PropertyInfoNameComparer.Instance,
                navigationProperties.Select(n => new KeyValuePair<PropertyInfo, Type>(n, targetType)));

            var currentAmbiguousNavigations = GetAmbigousNavigations(entityTypeBuilder.Metadata);
            if (currentAmbiguousNavigations != null)
            {
                newAmbiguousNavigations = currentAmbiguousNavigations.AddRange(newAmbiguousNavigations);
            }

            SetAmbigousNavigations(entityTypeBuilder, newAmbiguousNavigations);
        }

        private static bool RemoveAmbiguous(EntityType entityType, PropertyInfo navigationProperty)
        {
            var ambigousNavigations = GetAmbigousNavigations(entityType);
            if (ambigousNavigations != null)
            {
                var currentCount = ambigousNavigations.Count;
                ambigousNavigations = ambigousNavigations.Remove(navigationProperty);
                if (currentCount != ambigousNavigations.Count)
                {
                    SetAmbigousNavigations(entityType.Builder, ambigousNavigations);
                    return true;
                }
            }

            return false;
        }

        private static void SetAmbigousNavigations(
            InternalEntityTypeBuilder entityTypeBuilder,
            ImmutableSortedDictionary<PropertyInfo, Type> ambiguousNavigations)
            => entityTypeBuilder.HasAnnotation(AmbiguousNavigationsAnnotationName, ambiguousNavigations, ConfigurationSource.Convention);

        private class PropertyInfoNameComparer : IComparer<PropertyInfo>
        {
            public static readonly PropertyInfoNameComparer Instance = new PropertyInfoNameComparer();

            private PropertyInfoNameComparer()
            {
            }

            public int Compare(PropertyInfo x, PropertyInfo y) => StringComparer.Ordinal.Compare(x.Name, y.Name);
        }

        private class RelationshipCandidate
        {
            public RelationshipCandidate(
                InternalEntityTypeBuilder targetTypeBuilder,
                HashSet<PropertyInfo> navigations,
                HashSet<PropertyInfo> inverseNavigations)
            {
                TargetTypeBuilder = targetTypeBuilder;
                NavigationProperties = navigations;
                InverseProperties = inverseNavigations;
            }

            public InternalEntityTypeBuilder TargetTypeBuilder { get; }
            public HashSet<PropertyInfo> NavigationProperties { get; }
            public HashSet<PropertyInfo> InverseProperties { get; }
        }
    }
}
